using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class BossController : MonoBehaviour
{
    public enum BossMode { CinematicChase, CombatBoss }
    [Header("Boss Configuration")]
    public BossMode mode = BossMode.CombatBoss;

    [Header("Common Settings")]
    public Transform player;
    public float chaseSpeed = 5f;

    [Header("Cinematic Settings (Damon)")]
    public CanvasGroup blackScreen;
    public string nextScene = "Map1_Clinic";

    [Header("Combat Settings (Reaper)")]
    public float attackRange = 3f;
    public float timeBetweenAttacks = 4f;
    public float windUpTime = 0.8f;
    public float recoveryTime = 1.5f;
    public float attackDamage = 20f;
    public float maxHealthOverride = 60f;

    [Header("Summoning")]
    public GameObject summonPrefab;
    public Transform[] summonPoints;
    public float summonHealthThreshold = 0.5f;

    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Color originalColor;
    private EnemyHealth enemyHealth;
    private BossHealth bossHealth;
    private float lastAttackTime;
    private bool isAttacking = false;
    private bool hasSummoned = false;
    private bool isEnding = false;
    private float lockedY;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.maxHealth = maxHealthOverride;
            enemyHealth.currentHealth = maxHealthOverride;
        }
        bossHealth = GetComponent<BossHealth>();
        lockedY = transform.position.y;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (mode == BossMode.CinematicChase)
        {
            if (blackScreen != null)
            {
                blackScreen.alpha = 0;
                blackScreen.blocksRaycasts = false;
            }
            if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void Update()
    {
        if (player == null || isEnding) return;
        
        // Check if dead
        if ((enemyHealth != null && enemyHealth.currentHealth <= 0) || 
            (bossHealth != null && bossHealth.health <= 0)) 
            return;

        if (mode == BossMode.CinematicChase)
        {
            UpdateCinematicChase();
        }
        else
        {
            UpdateCombatBoss();
        }
    }

    // --- CINEMATIC LOGIC (Damon / Tutorial) ---
    void UpdateCinematicChase()
    {
        float newX = Mathf.MoveTowards(transform.position.x, player.position.x, chaseSpeed * Time.deltaTime);
        transform.position = new Vector3(newX, lockedY, transform.position.z);

        // Flip sprite to face player
        float dir = (player.position.x > transform.position.x) ? 1 : -1;
        transform.localScale = new Vector3(dir, 1, 1);

        // THE TRIGGER: Once he is 0.8 units away, start the fade
        float distance = Mathf.Abs(player.position.x - transform.position.x);
        if (distance < 0.8f) 
        {
            StartCoroutine(ForceEndGame());
        }
    }

    IEnumerator ForceEndGame()
    {
        isEnding = true;
        if (blackScreen != null) 
        {
            blackScreen.alpha = 1;
            blackScreen.blocksRaycasts = true;
        }
        yield return new WaitForSeconds(2.5f);
        SceneManager.LoadScene(nextScene);
    }

    // --- COMBAT LOGIC (Reaper / Scene 9) ---
    void UpdateCombatBoss()
    {
        if (isAttacking) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Summoning Logic: Check health percentage
        float healthPercent = GetHealthPercent();
        if (!hasSummoned && healthPercent > 0 && healthPercent <= summonHealthThreshold)
        {
            StartCoroutine(PerformSummon());
            return;
        }

        if (distance <= attackRange)
        {
            if (Time.time >= lastAttackTime + timeBetweenAttacks)
            {
                StartCoroutine(PerformAttack());
            }
            else
            {
                StopMoving();
            }
        }
        else
        {
            MoveTowardPlayer();
        }
    }

    float GetHealthPercent()
    {
        if (enemyHealth != null) return enemyHealth.currentHealth / enemyHealth.maxHealth;
        if (bossHealth != null && bossHealth.healthSlider != null) return (float)bossHealth.health / bossHealth.healthSlider.maxValue;
        return 1f;
    }

    void MoveTowardPlayer()
    {
        if (anim != null) anim.SetBool("isChasing", true);
        float dir = (player.position.x > transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);
        
        // Flip logic (assumes original scale is positive)
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * (dir > 0 ? 1 : -1), transform.localScale.y, 1);
    }

    void StopMoving()
    {
        if (anim != null) anim.SetBool("isChasing", false);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        StopMoving();
        
        if (anim != null) anim.SetTrigger("attack");
        
        // --- VISUAL PULSE DURING WIND-UP ---
        float elapsed = 0f;
        while (elapsed < windUpTime)
        {
            if (sr != null)
            {
                // Pulse 3 times per second
                float t = Mathf.PingPong(elapsed * 6f, 1f); 
                sr.color = Color.Lerp(originalColor, Color.red, t);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (sr != null) sr.color = originalColor;


        // Damage calculation
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= attackRange + 1.5f) 
            {
                Debug.Log($"<color=red>BOSS HIT PLAYER for {attackDamage}!</color>");
                
                // Try New KnightHero script first
                KnightHero knight = player.GetComponent<KnightHero>();
                if (knight != null)
                {
                    knight.TakeDamage(attackDamage);
                    Vector2 knockbackDir = (player.position - transform.position).normalized;
                    knight.ApplyKnockback(new Vector2(knockbackDir.x * 10f, 5f));
                }
                else
                {
                    // Fallback to SendMessage for other scripts
                    player.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        // Wait for recovery
        yield return new WaitForSeconds(recoveryTime);
        
        lastAttackTime = Time.time;
        isAttacking = false;
    }

    IEnumerator PerformSummon()
    {
        hasSummoned = true;
        isAttacking = true;
        StopMoving();

        if (anim != null) anim.SetTrigger("summon");
        
        // Pause to let the summon animation play
        yield return new WaitForSeconds(1.2f);

        if (summonPrefab != null && summonPoints != null)
        {
            foreach (Transform point in summonPoints)
            {
                if (point == null) continue;
                GameObject minion = Instantiate(summonPrefab, point.position, Quaternion.identity);
                SummonAI ai = minion.GetComponent<SummonAI>();
                if (ai != null) ai.SetTarget(player);
            }
        }

        yield return new WaitForSeconds(0.8f);
        isAttacking = false;
    }

    // --- API for other scripts ---
    public void DisableBoss() { this.enabled = false; }
    public void HitPlayer() { } 
    public void EndAttack() { }
}