using UnityEngine;
using System.Collections;

public class CeilingTrap : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 25f;
    public float dropSpeed = 18f;
    public float resetSpeed = 2f;
    public float waitTimeAtBottom = 1.5f;
    [Header("Automatic Mode")]
    [Header("Automatic Mode")]
    public bool isAutomatic = true;
    public float autoInterval = 4f;

    [Header("Detection Settings")]
    public float detectionRange = 8f;
    public LayerMask playerLayer;
    public LayerMask groundLayer;

    private Vector3 initialPosition;
    private bool isActive = false;
    private float nextTriggerTime = 0f;
    private bool hasDealtDamage = false; 
    private Animator anim;

    void Start()
    {
        initialPosition = transform.position;
        anim = GetComponent<Animator>();
        
        // Auto-assign layers if not set
        if (playerLayer == 0) playerLayer = LayerMask.GetMask("Player");
        if (groundLayer == 0) groundLayer = LayerMask.GetMask("Ground");

        // Rigidbody2D is REQUIRED for moving triggers to work reliably
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        // BoxCollider2D is REQUIRED for the trap to hit things
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

        // Force Z to 0 to ensure it can hit the player
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        // Start automatic loop if enabled
        if (isAutomatic)
        {
            StartCoroutine(AutoTriggerLoop());
        }
    }

    void Update()
    {
        // Only run detection if NOT in automatic mode
        if (!isAutomatic && !isActive && Time.time >= nextTriggerTime)
        {
            DetectPlayer();
        }
    }

    IEnumerator AutoTriggerLoop()
    {
        // Initial random offset so they don't all fall at once
        yield return new WaitForSeconds(Random.Range(0f, 1.5f));

        while (true)
        {
            if (!isActive)
            {
                yield return StartCoroutine(TrapSequence());
            }
            yield return new WaitForSeconds(autoInterval);
        }
    }

    void DetectPlayer()
    {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y - 0.5f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, detectionRange, playerLayer);
        
        if (hit.collider != null)
        {
            StartCoroutine(TrapSequence());
        }
    }

    private bool isFalling = false;

    IEnumerator TrapSequence()
    {
        isActive = true;
        hasDealtDamage = false;
        isFalling = true; // Start falling

        if (anim != null) anim.SetTrigger("Drop");

        float groundY = FindGroundY();
        
        // 1. DROP
        while (transform.position.y > groundY + 0.55f)
        {
            transform.position += Vector3.down * dropSpeed * Time.deltaTime;
            yield return null;
        }

        // Disable damage INSTANTLY at bottom
        isFalling = false;

        // 2. WAIT at bottom
        yield return new WaitForSeconds(waitTimeAtBottom);

        // 3. RESET (Moving up)
        if (anim != null) anim.SetTrigger("Reset");
        while (Vector3.Distance(transform.position, initialPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, initialPosition, resetSpeed * Time.deltaTime);
            yield return null;
        }
        
        transform.position = initialPosition;
        
        // Short safety buffer before finishing
        yield return new WaitForSeconds(0.1f);
        
        isActive = false;
    }

    float FindGroundY()
    {
        // Start looking for ground 1 unit below the trap to avoid hitting itself
        Vector2 origin = new Vector2(transform.position.x, transform.position.y - 1f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 20f, groundLayer);
        return hit.collider != null ? hit.point.y : transform.position.y - 5f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"<color=cyan>Trap touched something: {collision.name}</color>");
        HandleDamage(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        HandleDamage(collision);
    }

    void HandleDamage(Collider2D collision)
    {
        // ONLY deal damage if the trap is actually falling and hasn't hit yet
        if (isFalling && !hasDealtDamage && (collision.CompareTag("Player") || collision.GetComponent<KnightHero>() != null))
        {
            KnightHero player = collision.GetComponent<KnightHero>();
            if (player != null)
            {
                Debug.Log("<color=green>TRAP SUCCESSFULLY HIT PLAYER!</color>");
                hasDealtDamage = true;
                player.TakeDamage(damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector2.down * detectionRange);
    }
}
