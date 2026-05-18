using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class MISTankBoss : MonoBehaviour
{
    [Header("Animation Control")]
    [Tooltip("Uncheck this if you haven't added Animation Events to your clips!")]
    public bool useManualAnimationEvents = false;

    [Header("Manual Animation Slots")]
    public AnimationClip idleClip;
    public AnimationClip walkClip;
    public AnimationClip attack1Clip;
    public AnimationClip hitClip;
    public AnimationClip deathClip;

    [Header("Player Target")]
    public Transform manualPlayerSlot;

    [Header("Stats")]
    public float maxHealth = 500f;
    public float moveSpeed = 3f;
    public float attackRange = 5f;
    public float chaseRange = 20f;
    public float attackCooldown = 3.0f; // Set to 3 seconds
    public float attackDamage = 15f;

    [Header("VFX")]
    public GameObject paperEffectPrefab;

    private Rigidbody2D rb;
    private PlayableGraph graph;
    private AnimationClipPlayable currentPlayable;
    private Transform player;
    private EnemyHealth health;
    private SpriteRenderer sr;
    
    private bool isDead = false;
    private bool isAttacking = false;
    private float nextAttackTime = 0f;
    private float lastHealth;
    private string currentState = "None";
    private Color originalColor;

    public void TriggerDamage()
    {
        DealDamage();
    }

    void OnEnable()
    {
        FindPlayer();
        SetupPlayableGraph();
    }

    void SetupPlayableGraph()
    {
        if (graph.IsValid()) graph.Destroy();
        graph = PlayableGraph.Create("BossGraph_" + gameObject.name);
        var animator = GetComponentInChildren<Animator>();
        if (animator == null) animator = gameObject.AddComponent<Animator>();
        AnimationPlayableOutput.Create(graph, "Animation", animator);
        graph.Play();
    }

    void Start()
    {
        useManualAnimationEvents = false; // FORCE automatic damage since user has raw clips

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        health = GetComponent<EnemyHealth>();
        if (health == null) health = gameObject.AddComponent<EnemyHealth>();
        
        health.maxHealth = maxHealth;
        health.currentHealth = maxHealth;
        lastHealth = maxHealth;

        // Force set layer to Enemy
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) gameObject.layer = enemyLayer;

        FindPlayer();

        if (rb != null) 
        {
            rb.freezeRotation = true;
            rb.gravityScale = 3f;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        IgnorePlayerPhysics();
        if (idleClip != null) PlayAnim(idleClip, "Idle");
    }

    void FindPlayer()
    {
        if (manualPlayerSlot != null) player = manualPlayerSlot;
        else
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj == null) { KnightHero kh = FindAnyObjectByType<KnightHero>(); if (kh != null) pObj = kh.gameObject; }
            if (pObj != null) player = pObj.transform;
        }
    }

    void Update()
    {
        if (isDead || !graph.IsValid()) return;
        if (health.currentHealth <= 0) { Die(); return; }
        if (health.currentHealth < lastHealth) { OnHit(); lastHealth = health.currentHealth; }
        if (isAttacking || player == null) return;

        float distanceX = Mathf.Abs(transform.position.x - player.position.x);
        float distanceY = Mathf.Abs(transform.position.y - player.position.y);

        if (distanceX <= attackRange && distanceY < 4f)
        {
            if (Time.time >= nextAttackTime) StartCoroutine(AttackSequence());
            else StopMoving();
        }
        else if (distanceX <= chaseRange) MoveTowardsPlayer();
        else StopMoving();
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
        float direction = player.position.x > transform.position.x ? 1 : -1;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);
        if (rb != null) rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        PlayAnim(walkClip, "Walk");
    }

    void StopMoving()
    {
        if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        PlayAnim(idleClip, "Idle");
    }

    IEnumerator AttackSequence()
    {
        isAttacking = true;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 1-second pulse before hitting
        yield return StartCoroutine(PulseWhite(1.0f));

        PlayAnim(attack1Clip, "Attack 1");

        if (useManualAnimationEvents)
        {
            yield return new WaitForSeconds(1.15f);
        }
        else
        {
            yield return new WaitForSeconds(0.65f); DealDamage();
            yield return new WaitForSeconds(0.13f); DealDamage();
            yield return new WaitForSeconds(0.12f); DealDamage();
            yield return new WaitForSeconds(0.25f);
        }

        isAttacking = false;
        nextAttackTime = Time.time + attackCooldown;
        currentState = "Recover";
    }

    IEnumerator PulseWhite(float duration)
    {
        if (sr == null) yield break;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Flash between original and pure white
            float t = Mathf.PingPong(elapsed * 10f, 1f);
            sr.color = Color.Lerp(originalColor, Color.white * 2f, t); // Bright white pulse
            elapsed += Time.deltaTime;
            yield return null;
        }
        sr.color = originalColor;
    }

    void DealDamage()
    {
        if (player == null) return;

        float dist = Mathf.Abs(transform.position.x - player.position.x);
        Debug.Log($"<color=orange>Boss trying to hit... Player is {dist} units away.</color>");

        if (dist <= attackRange + 2f)
        {
            KnightHero kh = player.GetComponent<KnightHero>();
            if (kh == null) kh = player.GetComponentInParent<KnightHero>();
            if (kh == null) kh = player.GetComponentInChildren<KnightHero>();

            if (kh != null) 
            {
                Debug.Log("<color=red>Boss landed hit on KnightHero!</color>");
                kh.TakeDamage(attackDamage);
            }
            else
            {
                Debug.LogWarning("Boss is close enough, but couldn't find KnightHero script on Player!");
            }
        }
        else
        {
            Debug.Log("<color=yellow>Boss missed! Player ran away during the attack windup.</color>");
        }
    }

    public void OnHit() { if (!isDead) { PlayAnim(hitClip, "Hit"); Invoke(nameof(ResetState), 0.3f); } }
    void ResetState() { if (!isAttacking) currentState = "None"; }

    void Die()
    {
        isDead = true;
        if (rb != null) rb.linearVelocity = Vector2.zero;
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
    void OnDestroy() { if (graph.IsValid()) graph.Destroy(); }
}
