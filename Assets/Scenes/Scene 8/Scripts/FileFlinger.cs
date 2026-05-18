using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Collections;

public class FileFlinger : MonoBehaviour
{
    [Header("Combat Settings")]
    public GameObject folderPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    public float projectileSpeed = 10f;
    public float detectionRange = 15f;
    public float moveSpeed = 3f;
    public float hoverDistance = 6f;

    [Header("Manual Animation Slots")]
    public AnimationClip flightClip;
    public AnimationClip attackClip;
    public AnimationClip takeHitClip;
    public AnimationClip deathClip;

    private Transform player;
    private float nextFireTime;
    
    private EnemyHealth health;
    private float lastHealth;
    private bool isDead = false;
    private bool isAttacking = false;

    private PlayableGraph graph;
    private AnimationClipPlayable currentPlayable;
    private string currentState = "None";

    void OnEnable()
    {
        SetupPlayableGraph();
    }

    void SetupPlayableGraph()
    {
        if (graph.IsValid()) graph.Destroy();
        graph = PlayableGraph.Create("FlingerGraph_" + gameObject.name);
        var animator = GetComponentInChildren<Animator>();
        if (animator == null) animator = gameObject.AddComponent<Animator>();
        AnimationPlayableOutput.Create(graph, "Animation", animator);
        graph.Play();
    }

    void PlayAnim(AnimationClip clip, string stateName)
    {
        if (clip == null || !graph.IsValid() || currentState == stateName) return;
        if (currentPlayable.IsValid()) currentPlayable.Destroy();
        currentPlayable = AnimationClipPlayable.Create(graph, clip);
        graph.GetOutput(0).SetSourcePlayable(currentPlayable);
        currentState = stateName;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) { KnightHero kh = FindAnyObjectByType<KnightHero>(); if (kh != null) player = kh.transform; }

        health = GetComponent<EnemyHealth>();
        if (health == null) health = gameObject.AddComponent<EnemyHealth>();
        
        health.autoDestroy = false; // Let FileFlinger handle its own death
        lastHealth = health.currentHealth;
        
        // Force Enemy Layer so the Sword can actually hit it!
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) gameObject.layer = enemyLayer;
        
        IgnorePlayerPhysics(); // Stop it from pushing the player!

        PlayAnim(flightClip, "Flight");
    }

    void IgnorePlayerPhysics()
    {
        if (player == null) return;
        foreach (var myCol in GetComponentsInChildren<Collider2D>())
        {
            foreach (var pCol in player.GetComponentsInChildren<Collider2D>())
            {
                Physics2D.IgnoreCollision(myCol, pCol, true);
            }
        }
    }

    void Update()
    {
        if (isDead || !graph.IsValid()) return;
        
        if (health.currentHealth <= 0) { Die(); return; }
        
        if (health.currentHealth < lastHealth) 
        { 
            OnHit(); 
            lastHealth = health.currentHealth; 
        }

        if (isAttacking || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        
        // Face player (Flip X)
        float direction = player.position.x > transform.position.x ? -1 : 1;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);

        // Movement Logic
        var rb = GetComponent<Rigidbody2D>();
        if (distance <= detectionRange)
        {
            if (distance > hoverDistance)
            {
                // Chase the player
                Vector2 moveDir = (player.position - transform.position).normalized;
                if (rb != null) rb.linearVelocity = moveDir * moveSpeed;
            }
            else
            {
                // Reached hover distance, stop moving
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }

            // Attack Logic
            if (Time.time >= nextFireTime)
            {
                StartCoroutine(AttackSequence());
            }
        }
        else
        {
            // Player is out of range, stop moving
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }

    IEnumerator AttackSequence()
    {
        isAttacking = true;
        PlayAnim(attackClip, "Attack");
        
        // Wind up
        yield return new WaitForSeconds(0.4f);
        Shoot();

        // Finish attack animation
        yield return new WaitForSeconds(0.6f);
        
        PlayAnim(flightClip, "Flight");
        nextFireTime = Time.time + fireRate;
        isAttacking = false;
    }

    void Shoot()
    {
        if (folderPrefab == null || firePoint == null || isDead) return;

        GameObject folder = Instantiate(folderPrefab, firePoint.position, Quaternion.identity);
        Vector2 direction = (player.position - firePoint.position).normalized;
        
        Rigidbody2D rb = folder.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * projectileSpeed;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            folder.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public void OnHit()
    {
        if (isDead) return;
        PlayAnim(takeHitClip, "Hit");
        CancelInvoke(nameof(ResetToFlight));
        Invoke(nameof(ResetToFlight), 0.3f);
    }
    
    void ResetToFlight()
    {
        if (!isDead && !isAttacking) PlayAnim(flightClip, "Flight");
    }

    void Die()
    {
        isDead = true;
        PlayAnim(deathClip, "Death");
        
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        
        foreach (var col in GetComponentsInChildren<Collider2D>()) col.enabled = false;
        Destroy(gameObject, 2f); // Despawn after 2 seconds
    }

    void OnDisable() { if (graph.IsValid()) graph.Destroy(); }
    void OnDestroy() { if (graph.IsValid()) graph.Destroy(); }
}
