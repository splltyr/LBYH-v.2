using UnityEngine;

public class KnightHero : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;

    [Header("Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded;
    private bool isDashing;
    private bool isAttacking;
    private float moveInput;
    private string currentAnimState; 

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.gravityScale = 3f; 
    }

    void Update() {
        if (isDashing) return;

        moveInput = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.GetMouseButtonDown(0) && !isAttacking) StartCoroutine(PerformAttack());
        if (Input.GetKeyDown(KeyCode.C) && !isDashing && !isAttacking) StartCoroutine(PerformDash());
        if (Input.GetButtonDown("Jump") && isGrounded && !isAttacking) {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (!isAttacking && !isDashing) {
            if (!isGrounded) ChangeAnimationState("KnightJump");
            else if (Mathf.Abs(moveInput) > 0.1f) ChangeAnimationState("KnightRun");
            else ChangeAnimationState("KnightIdle");
        }

        if (!isAttacking) {
            if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
            else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void FixedUpdate() {
        if (isDashing || isAttacking) return; 
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    void ChangeAnimationState(string newState) {
        if (currentAnimState == newState) return; 
        anim.Play(newState);
        currentAnimState = newState;
    }

    // THE FIX: This stops the player when Damon catches them
    void OnDisable() {
        if (rb != null) rb.linearVelocity = Vector2.zero; 
        if (anim != null) anim.Play("KnightIdle");
        currentAnimState = "KnightIdle";
    }

    System.Collections.IEnumerator PerformAttack() {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero; 
        ChangeAnimationState("KnightAttack");
        yield return new WaitForSeconds(0.2f); 
        isAttacking = false;
        currentAnimState = ""; 
    }

    System.Collections.IEnumerator PerformDash() {
        isDashing = true;
        ChangeAnimationState("KnightSlide");
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        float dashDir = transform.localScale.x;
        rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);
        yield return new WaitForSeconds(dashDuration);
        rb.gravityScale = originalGravity;
        isDashing = false;
        currentAnimState = ""; 
    }
}