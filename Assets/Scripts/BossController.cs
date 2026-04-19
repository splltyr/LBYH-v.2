using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Targeting")]
    public Transform player;         
    public float chaseSpeed = 3f;       
    public float attackRange = 2.5f; 

    [Header("Attack Rhythm")]
    [SerializeField] private float timeBetweenAttacks = 4f; 
    [SerializeField] private float windUpTime = 0.8f;      
    [SerializeField] private float recoveryTime = 1.5f;    
    private float attackTimer;

    [Header("Summoning (Phase 2)")]
    public GameObject summonPrefab;      
    public Transform[] summonPoints;     
    [SerializeField] private float summonHealthThreshold = 0.5f; 
    private bool hasSummoned = false;

    [Header("State Control")]
    private bool isExecutingMove = false;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private EnemyHealth healthScript;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        healthScript = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();
    // Add this line to force rotation off via code
    rb.freezeRotation = true;
        
        attackTimer = timeBetweenAttacks; 
    }

    void Update()
    {
        if (player == null || isExecutingMove || !this.enabled) return;

        // PHASE 2 CHECK: Summon minions when health is low
        if (!hasSummoned && healthScript != null)
        {
            if (healthScript.GetCurrentHealthPercentage() <= summonHealthThreshold)
            {
                StartCoroutine(SummonSequence());
                return;
            }
        }

        attackTimer -= Time.deltaTime;
        float distance = Vector2.Distance(transform.position, player.position);

        if (attackTimer <= 0 && distance <= attackRange)
        {
            StartCoroutine(GreatAttackSequence());
        }
        else
        {
            ChasePlayer();
        }
    }

    private IEnumerator SummonSequence()
    {
        isExecutingMove = true;
        hasSummoned = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isChasing", false);

        // Play summon animation
        anim.SetTrigger("summon"); 
        
        // Wait for roar/summon animation to finish
        yield return new WaitForSeconds(2f); 

        isExecutingMove = false;
    }

    // Called by Animation Event in the 'summon' clip
    public void SpawnSummonMinions()
    {
        if (summonPrefab == null) return;
        foreach (Transform point in summonPoints)
        {
            if (point != null)
            {
                GameObject minion = Instantiate(summonPrefab, point.position, Quaternion.identity);
                SummonAI ai = minion.GetComponent<SummonAI>();
                if (ai != null) ai.SetTarget(player);
            }
        }
    }

    private IEnumerator GreatAttackSequence()
    {
        isExecutingMove = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isChasing", false);

        // Wind-up Tell
        float elapsed = 0;
        while (elapsed < windUpTime)
        {
            sr.color = (elapsed % 0.2f > 0.1f) ? Color.red : Color.white;
            elapsed += Time.deltaTime;
            yield return null;
        }
        sr.color = Color.white;

        anim.SetTrigger("attack"); 
        yield return new WaitForSeconds(0.4f); // Animation impact delay
        yield return new WaitForSeconds(recoveryTime); // Tired phase

        attackTimer = timeBetweenAttacks;
        isExecutingMove = false;
    }

  void ChasePlayer()
{
    float distanceToPlayer = Vector2.Distance(transform.position, player.position);

    // FIX: Only move if the Boss is FURTHER away than the attack range
    // We add a tiny buffer (0.2f) so he doesn't stop and start repeatedly
    if (distanceToPlayer > attackRange - 0.2f)
    {
        float direction = (player.position.x > transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);
        anim.SetBool("isChasing", true);
        
        // Face the player
        transform.localScale = new Vector3(direction * Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
    }
    else
    {
        // If he's close enough, stop moving so he doesn't jitter
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("isChasing", false);
    }
}

    public void DisableBoss()
    {
        StopAllCoroutines();
        this.enabled = false;
        isExecutingMove = true; 
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isChasing", false);
    }

    public void HitPlayer()
{
    if (player == null) return;
    if (Vector2.Distance(transform.position, player.position) <= attackRange + 1f)
    {
        // Deal damage to player
        PlayerMovement pMove = player.GetComponent<PlayerMovement>();
        if (pMove != null)
        {
            pMove.TakeDamage(20f); // Set your boss damage here
            
            // Knockback logic
            Vector2 dir = (player.position - transform.position).normalized;
            pMove.ApplyKnockback(new Vector2(dir.x * 40f, 10f)); 
        }
    }
}
}