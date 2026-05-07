using UnityEngine;

public class SchoolBullyAI : MonoBehaviour
{
    [Header("References")]
    public Transform player; 
    private Animator anim;
    private Rigidbody2D rb;

    [Header("Settings")]
    public float detectionRange = 7f;
    public float attackRange = 1.5f; // Close enough to punch
    public float walkSpeed = 2.5f;   // Speed before stealing key
    public float chaseSpeed = 5f;    // Speed after stealing key
    public float attackCooldown = 1.5f;
    
    private float lastAttackTime;
    private bool isEnraged = false; 
    private bool playerDetected = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        anim.SetInteger("State", 0); 
    }

    void FixedUpdate() // Using FixedUpdate for Rigidbody movement
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!playerDetected && distanceToPlayer <= detectionRange)
        {
            playerDetected = true;
            anim.SetInteger("State", 1); 
        }

        if (playerDetected)
        {
            LookAtPlayer();

            // 1. If too far to punch, move closer
            if (distanceToPlayer > attackRange)
            {
                MoveToTarget();
            }
            // 2. If close enough, stop and attack
            else 
            {
                StopMoving();
                TryAttack();
            }
        }
    }

    void MoveToTarget()
    {
        // Use Run animation if enraged, otherwise use Fight (or a walk if you had one)
        // Since we only have Fight and Run, we'll use Run for any movement
        anim.SetBool("IsRunning", true);

        float currentSpeed = isEnraged ? chaseSpeed : walkSpeed;
        Vector2 target = new Vector2(player.position.x, rb.position.y);
        Vector2 newPos = Vector2.MoveTowards(rb.position, target, currentSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
    }

    void StopMoving()
    {
        anim.SetBool("IsRunning", false);
    }

    void TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            anim.SetTrigger("Attack"); 
            lastAttackTime = Time.time;
        }
    }

    void LookAtPlayer()
    {
        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }

    // TRIGGER THIS VIA ANIMATION EVENT
    public void DealDamage()
    {
        // Check distance one last time during the punch frame
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange + 0.5f)
        {
            // player.GetComponent<PlayerHealth>().TakeDamage(10);
            Debug.Log("PLAYER HIT!");
        }
    }

    public void StealKeyCard()
    {
        isEnraged = true;
        Debug.Log("The School Bully: GIVE ME BACK MY KEY CARD!");
    }
}