using UnityEngine;
using System.Collections;

public class KnightHero : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;

    [Header("Combat Settings")]
    public Transform attackPoint;      
    public float attackRange = 1.25f;   
    public float attackDamage = 25f;   
    public LayerMask enemyLayers;     

    [Header("Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    [Header("Restrictions")]
    public bool canAttack = true; 
    public bool canDash = true;   

    [Header("Companion (Tala)")]
    public Transform talaTransform; // Drag the Tala/Glowy Bubble object here
    public Vector3 talaOffset = new Vector3(-1.5f, 1.5f, 0f); 
    public float followSpeed = 5f; 

    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded;
    private bool isDashing;
    private bool isAttacking;
    private float moveInput;
    private string currentAnimState; 

    void Awake() {
        // Ensure only one AudioListener exists in the scene
        AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (listeners.Length > 1)
        {
            bool keptOne = false;
            foreach (var listener in listeners)
            {
                if (listener.gameObject.CompareTag("MainCamera") && !keptOne)
                {
                    listener.enabled = true;
                    keptOne = true;
                }
                else
                {
                    listener.enabled = false;
                }
            }
            if (!keptOne && listeners.Length > 0) listeners[0].enabled = true;
        }

        if (attackPoint == null) {
            Transform found = transform.Find("AttackPoint");
            if (found != null)
                attackPoint = found;
            else {
                var go = new GameObject("AttackPoint");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0.85f, 0.35f, 0f);
                attackPoint = go.transform;
            }
        }
        if (enemyLayers.value == 0) {
            int enemy = LayerMask.NameToLayer("Enemy");
            if (enemy >= 0)
                enemyLayers = 1 << enemy;
        }
    }

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.gravityScale = 3f; 
        if (gameObject.tag != "Player") gameObject.tag = "Player";

        if (talaTransform != null) {
            var follow = talaTransform.GetComponent<TalaFollow>();
            if (follow != null && follow.playerTransform == null)
                follow.playerTransform = transform;
        }
    }

    void Update() {
        if (isDashing) return;

        moveInput = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Combat Input
        if (canAttack && Input.GetMouseButtonDown(0) && !isAttacking) StartCoroutine(PerformAttack());
        if (canDash && Input.GetKeyDown(KeyCode.C) && !isDashing && !isAttacking) StartCoroutine(PerformDash());
        
        if (Input.GetButtonDown("Jump") && isGrounded && !isAttacking) {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Animation Logic
        if (!isAttacking && !isDashing) {
            if (!isGrounded) ChangeAnimationState("KnightJump");
            else if (Mathf.Abs(moveInput) > 0.1f) ChangeAnimationState("KnightRun");
            else ChangeAnimationState("KnightIdle");
        }

        if (!isAttacking) {
            if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
            else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);
        }

        // --- TALA TRAIL LOGIC ---
        // If Tala uses TalaFollow, that script owns her position — do not double-move here.
        if (talaTransform != null && talaTransform.GetComponent<TalaFollow>() == null) {
            Vector3 flippedOffset = talaOffset;
            flippedOffset.x *= transform.localScale.x;
            Vector3 targetPos = transform.position + flippedOffset;
            talaTransform.position = Vector3.Lerp(talaTransform.position, targetPos, followSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate() {
        if (isDashing || isAttacking) return; 
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    IEnumerator PerformAttack() {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero; 
        ChangeAnimationState("KnightAttack");
        yield return new WaitForSeconds(0.1f); 

        if (attackPoint == null) yield break;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies) {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>() ?? enemy.GetComponentInParent<EnemyHealth>();
            BossHealth bossHealth = enemy.GetComponent<BossHealth>() ?? enemy.GetComponentInParent<BossHealth>();

            if (health != null) {
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                health.TakeDamage(attackDamage, knockbackDir);
            }
            else if (bossHealth != null) {
                bossHealth.TakeDamage((int)attackDamage);
            }
        }
        yield return new WaitForSeconds(0.2f); 
        isAttacking = false;
    }

    void ChangeAnimationState(string newState) {
        if (currentAnimState == newState || anim == null) return; 
        anim.Play(newState);
        currentAnimState = newState;
    }

    // Fixed OnDisable to prevent NullRefs and unnecessary type checks
    void OnDisable() {
        if (rb != null) rb.linearVelocity = Vector2.zero; 
        if (anim != null) {
            anim.Play("KnightIdle");
            currentAnimState = "KnightIdle";
        }
    }

    IEnumerator PerformDash() {
        isDashing = true;
        ChangeAnimationState("KnightSlide");
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        float dashDir = transform.localScale.x;
        rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);
        yield return new WaitForSeconds(dashDuration);
        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    public void TakeDamage(float damage)
    {
        Debug.Log($"<color=red>KNIGHT HIT! Damage: {damage}</color>");
        ChangeAnimationState("KnightHit"); // If you have a hit animation
        // Add health subtraction here if you add a health variable later
    }

    void OnDrawGizmosSelected() {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}