using UnityEngine;
using System.Collections;

public class JanitorBossController : MonoBehaviour
{
    [Header("Movement")]
    public float chargeSpeed = 18f;
    public float normalSpeed = 4f;
    public float stunDuration = 3.5f;
    
    [Header("Logic")]
    public bool isStunned = false;
    private bool isCharging = false;
    private Rigidbody2D rb;
    public Transform player; // Manually assign this in Inspector!
    private Animator anim;
    private EnemyHealth health;
    private Vector2 lastChargeDir;

    // Events for SceneSequence to listen to
    public System.Action OnCrashed;
    public System.Action OnRecovered;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();
        
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 4f; // Enable natural gravity for ground contact
        }
        
        // Prevent falling through floor trigger setup by solidifying colliders
        foreach (var col in GetComponentsInChildren<Collider2D>())
        {
            col.isTrigger = false;
        }
        
        // Only search for tag if the slot is left empty in Inspector
        if (player == null) 
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else Debug.LogError("<color=red>JanitorBoss: Player slot is empty and couldn't find 'Player' tag!</color>");
        }
        
        // Start the behavior loop
        StartCoroutine(BossAI());
    }

    IEnumerator BossAI()
    {
        // Give player a second to breathe
        yield return new WaitForSeconds(2f);

        while (health != null && health.currentHealth > 0)
        {
            if (isStunned)
            {
                rb.linearVelocity = Vector2.zero;
                if (anim != null) anim.Play("Idle"); // Use Idle for stunned
                yield return new WaitForSeconds(stunDuration);
                isStunned = false;
                OnRecovered?.Invoke();
            }

            // Phase 1: Pursuit
            float pursuitTimer = Random.Range(2f, 4f);
            while (pursuitTimer > 0 && !isStunned)
            {
                MoveTowardsPlayer();
                pursuitTimer -= Time.deltaTime;
                yield return null;
            }

            // Phase 2: Ground Slam (Visual Pause)
            rb.linearVelocity = Vector2.zero;
            if (anim != null) anim.Play("VineJab"); // Use VineJab for the slam warning
            yield return new WaitForSeconds(1.2f);

            // Phase 3: Charge
            yield return ChargeAtPlayer();
        }
    }

    void MoveTowardsPlayer()
    {
        if (player == null || isStunned || isCharging) return;
        Vector2 dir = (player.position - transform.position).normalized;
        // ONLY move horizontally! Let physics gravity handle Y axis contact with the floor.
        rb.linearVelocity = new Vector2(dir.x * normalSpeed, rb.linearVelocity.y);
        
        if (anim != null) anim.Play("Walk");

        // Face player
        if (dir.x > 0.1f) transform.localScale = new Vector3(-1, 1, 1);
        else if (dir.x < -0.1f) transform.localScale = new Vector3(1, 1, 1);
    }

    IEnumerator ChargeAtPlayer()
    {
        if (player == null || isStunned) yield break;
        
        isCharging = true;
        if (anim != null) anim.Play("Walk"); // Run really fast during charge
        
        // Lock in direction horizontally
        lastChargeDir = (player.position - transform.position).normalized;
        lastChargeDir.y = 0; // Lock to horizontal only!
        lastChargeDir = lastChargeDir.normalized;
        
        float chargeTimer = 0;
        float maxChargeTime = 2.5f;
        
        while (chargeTimer < maxChargeTime && isCharging && !isStunned)
        {
            rb.linearVelocity = new Vector2(lastChargeDir.x * chargeSpeed, rb.linearVelocity.y);
            chargeTimer += Time.deltaTime;
            yield return null;
        }
        
        isCharging = false;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCharging)
        {
            // If we hit the player, deal damage but keep charging!
            if (collision.gameObject.CompareTag("Player") || collision.gameObject == player?.gameObject)
            {
                collision.gameObject.SendMessage("TakeDamage", 35f, SendMessageOptions.DontRequireReceiver);
                Debug.Log("<color=red>Janitor Boss: HIT PLAYER!</color>");
            }
            // If we hit a wall/obstacle, STUN!
            else
            {
                ApplyStun();
            }
        }
    }

    public void ApplyStun()
    {
        if (isStunned) return;
        
        isStunned = true;
        isCharging = false;
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.Play("Hit"); // Play Hit when crashing
        
        OnCrashed?.Invoke();
        Debug.Log("<color=cyan>Janitor Boss: CRASHED AND STUNNED!</color>");
        
        // Small screen shake effect if you have a camera shaker
        // Camera.main.GetComponent<CameraShake>()?.Shake(0.2f, 0.5f);
    }

    public void Die()
    {
        StopAllCoroutines(); // Immediately stop the pursuit and charge behavior loops!
        isStunned = true;
        isCharging = false;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; // Freeze physics and prevent movement
        }
        
        foreach (var col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false; // Disable all hitboxes and collisions
        }
        
        if (anim != null) anim.Play("Human"); // Play healed human animation immediately
        Debug.Log("<color=green>JanitorBossController: Healed! Froze in place.</color>");
    }
}
