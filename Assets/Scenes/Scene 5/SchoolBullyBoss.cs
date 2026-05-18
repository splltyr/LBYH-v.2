using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Collections;

public class SchoolBullyBoss : MonoBehaviour
{
    [Header("Animation Slots (Drag .anim files here)")]
    public AnimationClip idleClip;
    public AnimationClip runClip;
    public AnimationClip attackClip;
    public AnimationClip dashClip;
    public AnimationClip hitClip;
    public AnimationClip deathClip;

    [Header("Stats")]
    public float maxHealth = 1000f;
    public float moveSpeed = 4f;
    public float dashSpeed = 15f;
    public float attackRange = 2.5f;
    public float dashRange = 10f;
    public float attackCooldown = 3f;
    public float punchDamage = 20f;
    public float dashDamage = 35f;

    [Header("Projectiles & VFX")]
    public GameObject digitalProjectilePrefab;
    public Transform throwPoint;

    private Rigidbody2D rb;
    private PlayableGraph graph;
    private AnimationClipPlayable currentPlayable;
    private Transform player;
    private EnemyHealth health;
    private SpriteRenderer sr;
    
    private bool isDead = false;
    private bool isActionActive = false;
    private float nextAttackTime = 0f;
    private float lastHealth;
    private string currentState = "None";
    private Color originalColor;

    void OnEnable() { SetupPlayableGraph(); }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        health = GetComponent<EnemyHealth>();
        if (health == null) health = gameObject.AddComponent<EnemyHealth>();
        
        health.maxHealth = maxHealth;
        health.currentHealth = maxHealth;
        lastHealth = maxHealth;

        FindPlayer();
        IgnorePlayerPhysics();

        if (rb != null) { rb.freezeRotation = true; rb.gravityScale = 4f; }
        
        // Prevent falling through the floor by solidifying colliders
        foreach (var col in GetComponentsInChildren<Collider2D>())
        {
            col.isTrigger = false;
        }
        
        PlayAnim(idleClip, "Idle");
        
        // Force Enemy Layer
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) gameObject.layer = enemyLayer;
    }

    void SetupPlayableGraph()
    {
        if (graph.IsValid()) graph.Destroy();
        graph = PlayableGraph.Create("BullyGraph_" + gameObject.name);
        var animator = GetComponentInChildren<Animator>();
        if (animator == null) animator = gameObject.AddComponent<Animator>();
        AnimationPlayableOutput.Create(graph, "Animation", animator);
        graph.Play();
    }

    void FindPlayer()
    {
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj == null) { KnightHero kh = FindAnyObjectByType<KnightHero>(); if (kh != null) pObj = kh.gameObject; }
        if (pObj != null) player = pObj.transform;
    }

    void Update()
    {
        if (isDead || !graph.IsValid()) return;
        if (health.currentHealth <= 0) { Die(); return; }
        if (health.currentHealth < lastHealth) { OnHit(); lastHealth = health.currentHealth; }
        if (isActionActive || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (Time.time >= nextAttackTime)
        {
            if (dist <= attackRange) StartCoroutine(PunchAttack());
            else if (dist <= dashRange) StartCoroutine(DashAttack());
            else StartCoroutine(ThrowAttack());
        }
        else if (dist > attackRange - 1f) MoveTowardsPlayer();
        else StopMoving();
    }

    IEnumerator PunchAttack()
    {
        isActionActive = true;
        StopMoving();
        yield return StartCoroutine(PulseWhite(1.0f));
        
        PlayAnim(attackClip, "Attack");
        yield return new WaitForSeconds(0.4f); // Impact frame
        
        if (Vector2.Distance(transform.position, player.position) <= attackRange + 1f)
        {
            KnightHero kh = player.GetComponent<KnightHero>();
            if (kh != null)
            {
                kh.TakeDamage(punchDamage);
                Vector2 knockback = (player.position - transform.position).normalized * 10f;
                kh.ApplyKnockback(knockback);
            }
        }

        yield return new WaitForSeconds(0.6f);
        FinishAction();
    }

    IEnumerator DashAttack()
    {
        isActionActive = true;
        StopMoving();
        yield return StartCoroutine(PulseWhite(1.0f));

        PlayAnim(dashClip, "Dash");
        Vector2 dashDir = (player.position - transform.position).normalized;
        dashDir.y = 0; // Only dash horizontally
        
        float dashTime = 0.6f;
        float elapsed = 0f;
        bool hasHit = false;

        while (elapsed < dashTime)
        {
            rb.linearVelocity = new Vector2(dashDir.x * dashSpeed, rb.linearVelocity.y);
            
            if (!hasHit && Vector2.Distance(transform.position, player.position) < 1.5f)
            {
                hasHit = true;
                KnightHero kh = player.GetComponent<KnightHero>();
                if (kh != null) { kh.TakeDamage(dashDamage); kh.ApplyKnockback(dashDir * 15f); }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.5f);
        FinishAction();
    }

    IEnumerator ThrowAttack()
    {
        isActionActive = true;
        StopMoving();
        yield return StartCoroutine(PulseWhite(0.8f));
        
        PlayAnim(attackClip, "Throw");
        if (digitalProjectilePrefab != null)
        {
            GameObject proj = Instantiate(digitalProjectilePrefab, throwPoint != null ? throwPoint.position : transform.position + Vector3.up, Quaternion.identity);
            // Projectile script should handle moving towards player
        }

        yield return new WaitForSeconds(0.5f);
        FinishAction();
    }

    void FinishAction()
    {
        isActionActive = false;
        nextAttackTime = Time.time + attackCooldown;
    }

    IEnumerator PulseWhite(float duration)
    {
        if (sr == null) yield break;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.PingPong(elapsed * 10f, 1f);
            sr.color = Color.Lerp(originalColor, Color.white * 2f, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        sr.color = originalColor;
    }

    void PlayAnim(AnimationClip clip, string stateName)
    {
        if (clip == null || !graph.IsValid() || currentState == stateName) return;
        if (currentPlayable.IsValid()) currentPlayable.Destroy();
        currentPlayable = AnimationClipPlayable.Create(graph, clip);
        graph.GetOutput(0).SetSourcePlayable(currentPlayable);
        currentState = stateName;
    }

    void MoveTowardsPlayer()
    {
        float dir = player.position.x > transform.position.x ? 1 : -1;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * dir, transform.localScale.y, transform.localScale.z);
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
        PlayAnim(runClip, "Run");
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        PlayAnim(idleClip, "Idle");
    }

    void OnHit() { if (!isDead && !isActionActive) { PlayAnim(hitClip, "Hit"); Invoke(nameof(ResetAnim), 0.3f); } }
    void ResetAnim() { if (!isActionActive) currentState = "None"; }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        PlayAnim(deathClip, "Death");
        foreach (var col in GetComponentsInChildren<Collider2D>()) col.enabled = false;
        Destroy(gameObject, 3f);
    }

    void IgnorePlayerPhysics()
    {
        if (player == null) return;
        foreach (var bCol in GetComponentsInChildren<Collider2D>())
            foreach (var pCol in player.GetComponentsInChildren<Collider2D>())
                Physics2D.IgnoreCollision(bCol, pCol, true);
    }

    void OnDisable() { if (graph.IsValid()) graph.Destroy(); }
}
