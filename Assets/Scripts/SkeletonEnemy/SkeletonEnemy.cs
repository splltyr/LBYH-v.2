using UnityEngine;

public class SkeletonEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float chaseRange = 7f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float stopBuffer = 0.3f;

    [Header("Combat Timing")]
    [SerializeField] private float attackCooldown = 4f; 
    private float nextAttackTime = 0f;

    private Animator anim;
    private Rigidbody2D rb;
    private Transform player;
    private bool isDead = false;
    private bool isAttacking = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
        if (rb != null) rb.freezeRotation = true;
        
        Physics2D.IgnoreLayerCollision(6, 9, true); 

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;
    }

    void Update()
    {
        EnemyHealth healthScript = GetComponent<EnemyHealth>();
        if (healthScript != null && healthScript.currentHealth <= 0) 
        {
            if (!isDead) Die();
            return;
        }

        if (isDead || player == null || isAttacking) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            StartAttack();
        }
        else if (distance <= chaseRange && distance > attackRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            StopMoving();
        }
    }

    // --- ANIMATION SIGNAL FUNCTIONS ---

    public void OnHit()
{
    if (isDead) return;

    // If the skeleton is currently attacking, ignore the flinch animation
    // This allows it to complete the swing (Super Armor)
    if (isAttacking) 
    {
        return; 
    }

    // Otherwise, play the hit animation and stop movement
    anim.SetTrigger("Hit");
    
    if (rb != null) 
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
}

    // Restores the logic to deal damage to player
    // Call this via Animation Event at the 'Contact' frame
    public void ExecuteDamage()
    {
        if (player == null || isDead) return;

        float distance = Vector2.Distance(transform.position, player.position);
        
        if (distance <= attackRange + 0.5f)
        {
            PlayerMovement pMove = player.GetComponent<PlayerMovement>();
            if (pMove != null)
            {
                pMove.TakeDamage(15f);
                
                // Adds knockback so the player feels the hit
                Vector2 knockbackDir = (player.position - transform.position).normalized;
                pMove.ApplyKnockback(new Vector2(knockbackDir.x * 15f, 8f));
            }
        }
    }

    // CRITICAL: Call this via Animation Event at the very LAST frame
    public void ResetAttack() 
    { 
        isAttacking = false; 
    }

    // --- MOVEMENT LOGIC ---

    void StartAttack()
    {
        StopMoving();
        isAttacking = true;
        anim.SetTrigger("Attack");
    }

    void MoveTowardsPlayer()
    {
        float directionX = player.position.x - transform.position.x;
        
        if (Mathf.Abs(directionX) < stopBuffer) 
        { 
            StopMoving(); 
            return; 
        }

        anim.SetBool("isWalking", true);
        transform.localScale = new Vector3(directionX > 0 ? 1 : -1, 1, 1);
        rb.linearVelocity = new Vector2((directionX > 0 ? 1 : -1) * moveSpeed, rb.linearVelocity.y);
    }

    void StopMoving()
    {
        anim.SetBool("isWalking", false);
        if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void Die()
    {
        isDead = true;
        anim.SetBool("isDead", true);
        if (rb != null) rb.linearVelocity = Vector2.zero;
        
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 3f);
    }
}