using UnityEngine;
using UnityEngine.UI; // THIS IS THE MISSING LINE!
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Health UI")]
[SerializeField] private Slider playerHealthBar;
[SerializeField] private float maxHealth = 100f;
private float currentHealth;
    [Header("Combat")]
    [SerializeField] private Transform attackPoint;    // Drag an empty GameObject here (child of Player)
    [SerializeField] private float attackRange = 1.2f;   // Size of the hit circle
    [SerializeField] private LayerMask enemyLayers;     // Set this to the "Enemy" layer
    [SerializeField] private float attackDamage = 25f;  // Base damage amount

    [Header("Movement Settings")]
    [SerializeField] private float speed = 10f;             
    [SerializeField] private float jumpingPower = 28f;      
    private float defaultGravity = 4.5f;                    

    [Header("Dodge Settings")]
    [SerializeField] private float dodgeForce = 22f;        
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float dodgeCooldown = 1.0f;    
    private bool canDodge = true;
    private bool isDodging = false;

    [Header("Technical References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Animator animator;

    // Internal State
    private float horizontal;
    private bool isFacingRight = true;
    private int jumpCount = 0;
    private int maxAirJumps = 1;
    private bool isStunned = false;
    [SerializeField] private float stunDuration = 0.4f;
    
    [HideInInspector] public bool isAttacking = false;

    void Start()
    {
        rb.gravityScale = defaultGravity;
        // Safety: Ensure these are reset on start
        canDodge = true;
        isDodging = false;
        isAttacking = false;
        currentHealth = maxHealth;
if (playerHealthBar != null)
{
    playerHealthBar.maxValue = maxHealth;
    playerHealthBar.value = maxHealth;
}
    }

    void Update()
    {
        // SAFETY RESET: Fixes the "can't dash at start" bug
        if (CheckGrounded() && !isDodging && !animator.GetCurrentAnimatorStateInfo(0).IsName("Dodge"))
        {
            canDodge = true; 
        }

        if (isStunned) return;

        // DASH ATTACK LOGIC
        if (isDodging)
        {
            if (Input.GetMouseButtonDown(0))
            {
                animator.SetTrigger("dashAttack"); 
            }
            return; 
        }

        horizontal = Input.GetAxisRaw("Horizontal");
        bool grounded = CheckGrounded();

        // DODGE INPUT (C Key)
        if (Input.GetKeyDown(KeyCode.C) && canDodge)
        {
            StartCoroutine(PerformDodge());
        }

        // ATTACK INPUT
        if (Input.GetMouseButtonDown(0) && !isAttacking) 
        {
            isAttacking = true;
            animator.SetTrigger("Attack"); 
        }

        // JUMPING logic
        if (grounded) jumpCount = 0;

        if (Input.GetButtonDown("Jump") && !isAttacking) 
        {
            if (grounded || jumpCount < maxAirJumps)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
                if (!grounded) jumpCount++; 
                animator.SetTrigger("Jump");
            }
        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        // ANIMATIONS
        animator.SetFloat("yVelocity", rb.linearVelocity.y);
        animator.SetBool("isWalking", horizontal != 0); 
        
        Flip();
    }

    private void FixedUpdate()
    {
        if (isStunned || isDodging) return;

        // FALL MULTIPLIER
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = defaultGravity * 1.5f;
        }
        else
        {
            rb.gravityScale = defaultGravity;
        }

        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
    }

    // UPDATED COMBAT LOGIC: Now calculates knockback direction
    public void DealDamage(float damageAmount)
    {
        float finalDamage = damageAmount > 0 ? damageAmount : attackDamage;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                // Calculate direction from player to enemy for knockback
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                health.TakeDamage(finalDamage, knockbackDir);
            }
        }
    }

    private IEnumerator PerformDodge()
    {
        canDodge = false;
        isDodging = true;
        animator.SetBool("isDodging", true);

        Physics2D.IgnoreLayerCollision(6, 7, true); 

        float dodgeDir = horizontal != 0 ? Mathf.Sign(horizontal) : (isFacingRight ? 1 : -1);
        rb.linearVelocity = new Vector2(dodgeDir * dodgeForce, 0f);
        rb.gravityScale = 0f; 

        yield return new WaitForSeconds(dodgeDuration);

        rb.gravityScale = defaultGravity;
        animator.SetBool("isDodging", false);
        isDodging = false;
        Physics2D.IgnoreLayerCollision(6, 7, false); 

        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }

    public void ApplyKnockback(Vector2 force)
    {
        StopAllCoroutines(); 
        ResetMovementState();
        
        isStunned = true;
        rb.linearVelocity = force; 
        Invoke("EndStun", stunDuration);
    }

    private void EndStun() => isStunned = false;

    private void ResetMovementState()
    {
        isDodging = false;
        isAttacking = false;
        rb.gravityScale = defaultGravity;
        animator.SetBool("isDodging", false);
        Physics2D.IgnoreLayerCollision(6, 7, false);
    }

    private bool CheckGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.5f, groundLayer);
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0 || !isFacingRight && horizontal > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }
    
    // Call this at the end of attack animations
    public void EndAttack()
    {
        isAttacking = false;
        isDodging = false; 
    }

    // Visual Gizmo to see attack range in Scene View
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    public void TakeDamage(float damage)
{
    currentHealth -= damage;
    if (playerHealthBar != null) playerHealthBar.value = currentHealth;

    // Trigger hit animation if you have one
    animator.SetTrigger("Hit"); 

    if (currentHealth <= 0)
    {
        // Handle Player Death (Restart scene or show Game Over)
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
}