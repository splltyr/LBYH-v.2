using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class SkeletonEnemy : MonoBehaviour
{
    [Header("Targeting")]
    [Tooltip("Drag your Player object here from the Hierarchy")]
    [SerializeField] private Transform targetPlayer; 

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float chaseRange = 7f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float stopBuffer = 0.3f;

    [Header("Combat Stats")]
    [SerializeField] private float damageAmount = 5;
    [SerializeField] private float knockbackForce = 15f;
    [SerializeField] private float attackCooldown = 4f;
    private float nextAttackTime = 0f;

    private Animator anim;
    private Rigidbody2D rb;
    private bool isDead = false;
    public bool IsDead => isDead;
    private bool isAttacking = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
        if (rb != null) rb.freezeRotation = true;
        
        // Prevents enemy-to-enemy physical blocking
        Physics2D.IgnoreLayerCollision(6, 9, true); 

        // Fallback: If you forgot to drag the player into the slot, it tries to find them via Tag
        if (targetPlayer == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) targetPlayer = pObj.transform;
        }
    }

    void Update()
    {
        EnemyHealth healthScript = GetComponent<EnemyHealth>();
        if (healthScript != null && healthScript.currentHealth <= 0)
        {
            if (!isDead) Die();
            return;
        }

        if (isDead || targetPlayer == null || isAttacking) return;

        float distance = Vector2.Distance(transform.position, targetPlayer.position);

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
        if (isDead || isAttacking) return;

        anim.SetTrigger("Hit");
        
        if (rb != null) 
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    // Call this via Animation Event at the 'Contact' frame
    public void ExecuteDamage()
    {
        if (targetPlayer == null || isDead) return;

        float distance = Vector2.Distance(transform.position, targetPlayer.position);
        Debug.Log($"<color=yellow>{gameObject.name} is swinging! Distance: {distance}</color>");
        
        // Only deal damage if the player is actually within the hitting zone
        if (distance <= attackRange + 1.5f)
        {
            // Try KnightHero (New Standard)
            KnightHero knight = targetPlayer.GetComponent<KnightHero>();
            if (knight != null)
            {
                Debug.Log($"<color=red>Skeleton HIT KnightHero for {damageAmount} damage!</color>");
                knight.TakeDamage(damageAmount);
                Vector2 knockbackDir = (targetPlayer.position - transform.position).normalized;
                knight.ApplyKnockback(new Vector2(knockbackDir.x * knockbackForce, 8f));
                return;
            }

            // Try PlayerMovement (Legacy)
            PlayerMovement pMove = targetPlayer.GetComponent<PlayerMovement>();
            if (pMove != null)
            {
                pMove.TakeDamage(damageAmount);
                Vector2 knockbackDir = (targetPlayer.position - transform.position).normalized;
                pMove.ApplyKnockback(new Vector2(knockbackDir.x * knockbackForce, 8f));
            }
        }
    }

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
        
        // Safety Fallback: Reset if animation event fails
        CancelInvoke(nameof(ResetAttack));
        Invoke(nameof(ResetAttack), 1.5f);
    }

    void MoveTowardsPlayer()
    {
        float directionX = targetPlayer.position.x - transform.position.x;
        
        if (Mathf.Abs(directionX) < stopBuffer) 
        { 
            StopMoving();
            return; 
        }

        anim.SetBool("isWalking", true);
        transform.localScale = new Vector3(directionX > 0 ? 1 : -1, 1, 1);
        
        if (rb != null)
        {
            rb.linearVelocity = new Vector2((directionX > 0 ? 1 : -1) * moveSpeed, rb.linearVelocity.y);
        }
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