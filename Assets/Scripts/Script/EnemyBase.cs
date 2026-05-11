using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Universal Detection")]
    public Transform playerTransform;
    public float moveSpeed = 3f;
    public float chaseRange = 10f;
    public float attackRange = 2f;

    [Header("State Flags")]
    public bool isDead = false;
    public bool isAttacking = false;
    public bool isTalking = false;

    // Protected means the "child" scripts (like Rome's Boss) can use them
    protected Animator anim;
    protected Rigidbody2D rb;
    protected EnemyHealth health;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();
        
        // Auto-find Yves in the scene if he isn't assigned
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform; 
    }

    protected virtual void Update()
    {
        // Global "stop" conditions
        if (isDead || playerTransform == null || isAttacking || isTalking) return; 

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= attackRange)
        {
            AttackLogic(); // The "Boop" part you'll fill in later
        }
        else if (distance <= chaseRange)
        {
            MoveToPlayer();
        }
        else
        {
            StopMoving();
        }
    }

    public virtual void MoveToPlayer()
    {
        float directionX = playerTransform.position.x - transform.position.x;
        
        // Handle flipping the sprite
        transform.localScale = new Vector3(directionX > 0 ? -1 : 1, 1, 1); 
        
        if (anim != null) anim.SetBool("isRunning", true);
        rb.linearVelocity = new Vector2((directionX > 0 ? 1 : -1) * moveSpeed, rb.linearVelocity.y);
    }

    public virtual void StopMoving()
    {
        if (anim != null) anim.SetBool("isRunning", false);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    // Abstract-style function: Child scripts MUST override this to do damage
    public virtual void AttackLogic() { } 
}