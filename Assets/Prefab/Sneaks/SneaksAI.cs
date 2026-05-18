using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class SneaksAI : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform targetPlayer; 

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.5f; // Fast for STEM Lab
    [SerializeField] private float chaseRange = 7f;
    [Tooltip("Max gap between this enemy's collider and the player's collider to start an attack (not center-to-center).")]
    [SerializeField] private float attackRange = 1.8f;

    [Header("Combat Timing")]
    [SerializeField] private float attackCooldown = 2.5f; // Prevents spamming
    private float nextAttackTime = 0f;

    [Header("Visuals")]
    [Tooltip("If set, flip uses localScale on this transform (preserves |x|). Otherwise uses SpriteRenderer.flipX.")]
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("Toggle if Sneaks faces the wrong way after flipping.")]
    [SerializeField] private bool invertFacing;

    private EnemyHealth health;
    private Animator anim;
    private Rigidbody2D rb;
    private Collider2D selfCollider;
    private Collider2D playerCollider;
    private bool isAttacking = false;
    // private bool hasStarted = false;

    void OnEnable()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
            rb.WakeUp();
            rb.linearVelocity = Vector2.zero; // Start fresh so gravity can take over
        }
        isAttacking = false;
    }

    void Start()
    {
        // hasStarted = true;
        health = GetComponent<EnemyHealth>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        selfCollider = GetComponent<Collider2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        if (spriteTransform == null && spriteRenderer != null)
            spriteTransform = spriteRenderer.transform;
        
        if (rb != null) rb.freezeRotation = true;

        if (targetPlayer == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) targetPlayer = pObj.transform;
        }

        CachePlayerCollider();
    }

    void CachePlayerCollider()
    {
        playerCollider = null;
        if (targetPlayer == null) return;
        playerCollider = targetPlayer.GetComponent<Collider2D>();
        if (playerCollider == null)
            playerCollider = targetPlayer.GetComponentInChildren<Collider2D>(true);
    }

    /// <summary>Shortest distance between Sneaks and player collider surfaces; negative if overlapping. Falls back to transform positions.</summary>
    float GetSeparationToPlayer()
    {
        if (selfCollider != null && playerCollider != null && selfCollider.enabled && playerCollider.enabled)
        {
            ColliderDistance2D d = Physics2D.Distance(selfCollider, playerCollider);
            return d.distance;
        }

        return Vector2.Distance(transform.position, targetPlayer.position);
    }

    void Update()
    {
        if (health != null && health.currentHealth <= 0f)
            return;

        if (targetPlayer == null)
            return;

        FaceTowardsPlayer();

        if (isAttacking)
            return;

        if (playerCollider == null || !playerCollider.enabled)
            CachePlayerCollider();

        float separation = GetSeparationToPlayer();

        // ATTACK LOGIC (With Cooldown to stop spamming)
        if (separation <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            StartAttack();
        }
        // CHASE LOGIC
        else if (separation <= chaseRange && separation > attackRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            StopMoving();
        }
    }

    void StartAttack()
    {
        StopMoving();
        isAttacking = true;
        CancelInvoke(nameof(ResetAttack));
        anim.SetTrigger("SneaksAttack"); // Synced with your animator
        // SneaksAttack.anim had no Animation Events; clear combat lock when clip ends.
        Invoke(nameof(ResetAttack), GetSneaksAttackClipLength());
    }

    float GetSneaksAttackClipLength()
    {
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips)
            {
                if (clip != null && clip.name == "SneaksAttack")
                    return clip.length;
            }
        }
        return 1.15f;
    }

    void FaceTowardsPlayer()
    {
        float dx = targetPlayer.position.x - transform.position.x;
        if (Mathf.Abs(dx) < 0.001f)
            return;

        bool faceLeft = dx < 0f;
        if (invertFacing)
            faceLeft = !faceLeft;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = faceLeft;
            return;
        }

        if (spriteTransform == null)
            return;

        float sign = faceLeft ? -1f : 1f;
        Vector3 s = spriteTransform.localScale;
        float ax = Mathf.Abs(s.x);
        if (ax < 0.0001f)
            ax = 1f;
        spriteTransform.localScale = new Vector3(sign * ax, s.y, s.z);
    }

    void MoveTowardsPlayer()
    {
        float directionX = targetPlayer.position.x - transform.position.x;
        anim.SetBool("isWalking", true); 

        if (rb != null)
            rb.linearVelocity = new Vector2((directionX > 0 ? 1 : -1) * moveSpeed, rb.linearVelocity.y);
    }

    void StopMoving()
    {
        anim.SetBool("isWalking", false);
        if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    // Call this via Animation Event at the 'Hit' frame
    public void ExecuteDamage()
    {
        if (targetPlayer == null || health == null || health.currentHealth <= 0f) return;
        
        float distance = Vector2.Distance(transform.position, targetPlayer.position);
        Debug.Log($"<color=yellow>Sneak is swinging! Distance: {distance}</color>");

        // Try KnightHero
        KnightHero knight = targetPlayer.GetComponent<KnightHero>();
        if (knight != null && distance <= attackRange + 1.5f)
        {
            Debug.Log("<color=red>Sneak HIT KnightHero!</color>");
            knight.TakeDamage(10f); // Fast enemy, lower damage
            Vector2 knockbackDir = (targetPlayer.position - transform.position).normalized;
            knight.ApplyKnockback(new Vector2(knockbackDir.x * 10f, 5f));
            return;
        }

        // Try Legacy PlayerMovement
        PlayerMovement pMove = targetPlayer.GetComponent<PlayerMovement>();
        if (pMove != null)
        {
            pMove.TakeDamage(10f);
            Vector2 knockbackDir = (targetPlayer.position - transform.position).normalized;
            pMove.ApplyKnockback(new Vector2(knockbackDir.x * 10f, 5f));
        }
    }

    // Call this via Animation Event at the 'End' of the attack clip (optional; Invoke also clears lock)
    public void ResetAttack() 
    { 
        isAttacking = false;
        CancelInvoke(nameof(ResetAttack));
        if (anim != null) anim.ResetTrigger("SneaksAttack");
    }

    void OnDisable()
    {
        CancelInvoke(nameof(ResetAttack));
    }
}