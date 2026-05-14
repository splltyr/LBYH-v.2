using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Health UI")]
    [SerializeField] private Slider playerHealthBar;
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Combat")]
    [SerializeField] private Transform attackPoint; 
    [SerializeField] private float attackRange = 1.2f; 
    [SerializeField] private LayerMask enemyLayers; 
    [SerializeField] private float attackDamage = 25f; 

    [Header("Movement Settings")]
    [SerializeField] private float speed = 10f; 
    [SerializeField] private float jumpingPower = 28f; 
    private float defaultGravity = 4.5f; 

    // Internal variable to track the "Active" speed used for cutscenes
    private float moveSpeed; 

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
        canDodge = true;
        isDodging = false;
        isAttacking = false;
        currentHealth = maxHealth;

        // Initialize moveSpeed with the inspector speed value
        moveSpeed = speed; 

        if (playerHealthBar != null)
        {
            playerHealthBar.maxValue = maxHealth;
            playerHealthBar.value = maxHealth;
        }
    }

    // --- FUNCTION CALLED BY DIALOGUE MANAGER ---
    public void SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
        
        // If frozen, immediately kill horizontal velocity
        if(newSpeed <= 0 && rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            horizontal = 0;
        }
    }

    void Update()
    {
        // Safety reset for dodging
        if (CheckGrounded() && !isDodging && !animator.GetCurrentAnimatorStateInfo(0).IsName("Dodge"))
        {
            canDodge = true; 
        }

        if (isStunned) return;

        // Block logic if dodging is active
        if (isDodging)
        {
            if (Input.GetMouseButtonDown(0)) animator.SetTrigger("dashAttack"); 
            return; 
        }

        // --- CUTSCENE CHECK: Only get input if speed is greater than 0 ---
        if (moveSpeed > 0)
        {
            horizontal = Input.GetAxisRaw("Horizontal");

            // DODGE INPUT
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

            // JUMPING
            bool grounded = CheckGrounded();
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
        }
        else
        {
            // Force zero horizontal if moveSpeed is 0
            horizontal = 0;
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

        // Fall Multiplier
        if (rb.linearVelocity.y < 0)
            rb.gravityScale = defaultGravity * 1.5f;
        else
            rb.gravityScale = defaultGravity;

        // Move based on the moveSpeed variable
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
    }

    public void DealDamage(float damageAmount)
    {
        float finalDamage = damageAmount > 0 ? damageAmount : attackDamage;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            BossHealth bossHealth = enemy.GetComponent<BossHealth>();

            if (health != null)
            {
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                health.TakeDamage(finalDamage, knockbackDir);
            }
            else if (bossHealth != null)
            {
                bossHealth.TakeDamage((int)finalDamage);
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

    private bool CheckGrounded() => Physics2D.OverlapCircle(groundCheck.position, 0.5f, groundLayer);

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
    
    public void EndAttack()
    {
        isAttacking = false;
        isDodging = false; 
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"<color=red>PLAYER HIT (Movement Script)! Damage: {damage}, Current Health: {currentHealth}</color>");
        if (playerHealthBar != null) playerHealthBar.value = currentHealth;
        animator.SetTrigger("Hit"); 

        if (currentHealth <= 0)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}