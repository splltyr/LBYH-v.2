using UnityEngine;

public class BullyEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.5f; 
    [SerializeField] private float chaseRange = 7f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float stopBuffer = 0.3f;

    [Header("Combat Timing")]
    [SerializeField] private float attackCooldown = 2.5f; 
    private float nextAttackTime = 0f;

    private Animator anim;
    private Rigidbody2D rb;
    private Transform player;
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isInDialogue = false; 

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
        if (rb != null) rb.freezeRotation = true;
        
        // Prevents the Bully from getting stuck on other NPCs or Enemies
        Physics2D.IgnoreLayerCollision(6, 9, true); 

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;
    }

    void Update()
    {
        // 1. HEALTH CHECK
        EnemyHealth healthScript = GetComponent<EnemyHealth>();
        if (healthScript != null && healthScript.currentHealth <= 0) 
        {
            if (!isDead) Die();
            return;
        }

        // 2. STATE GATES
        if (isDead || player == null || isAttacking || isInDialogue) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // 3. STANCE LOGIC: If in range, drop into the Fight Stance
        anim.SetBool("isFighting", distance <= chaseRange);

        // 4. BEHAVIOR BRANCHING
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

        // "Super Armor": Don't flinch if currently punching
        if (isAttacking) return; 

        anim.SetTrigger("Hit");
        StopMoving();
    }

    public void ExecuteDamage()
    {
        if (player == null || isDead) return;

        float distance = Vector2.Distance(transform.position, player.position);
        
        if (distance <= attackRange + 0.5f)
        {
            PlayerMovement pMove = player.GetComponent<PlayerMovement>();
            if (pMove != null)
            {
                pMove.TakeDamage(10f); // Adjust damage as needed
                
                // Feel the impact: Knockback Yves away from the bully
                Vector2 knockbackDir = (player.position - transform.position).normalized;
                pMove.ApplyKnockback(new Vector2(knockbackDir.x * 12f, 6f));
            }
        }
    }

    // CRITICAL: Call this via Animation Event at the VERY LAST frame of BullyAttack
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

        // Trigger Run Animation
        anim.SetBool("isRunning", true); 
        
        transform.localScale = new Vector3(directionX > 0 ? 1 : -1, 1, 1);
        
        if (rb != null)
            rb.linearVelocity = new Vector2((directionX > 0 ? 1 : -1) * moveSpeed, rb.linearVelocity.y);
    }

    void StopMoving()
    {
        anim.SetBool("isRunning", false);
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

    // Connect this to your Lab 201 Dialogue Manager
    public void SetDialogueState(bool active)
    {
        isInDialogue = active;
        if (active) StopMoving();
    }
}