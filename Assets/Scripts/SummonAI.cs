using UnityEngine;

public class SummonAI : MonoBehaviour
{
    [Header("Movement")]
    public float baseSpeed = 3.5f;
    public float accelerationRate = 0.5f; 
    private float currentSpeed;
    
    [Header("Effects")]
    public GameObject explosionEffect; 
    [SerializeField] private float selfDestructTime = 7f; // Safety timer
    
    private Transform target;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentSpeed = baseSpeed;
        
        if(rb != null) rb.freezeRotation = true; // Prevent spinning
        
        // Auto-explode after a few seconds if they don't reach Tyrone
        Invoke("Explode", selfDestructTime); 
    }

    public void SetTarget(Transform playerTransform)
    {
        target = playerTransform;
    }

    void Update()
    {
        if (target == null || isDead) 
        {
            if(rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        currentSpeed += accelerationRate * Time.deltaTime;

        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * currentSpeed;

        // Flip logic: Assumes sprite faces left by default
        if (direction.x != 0)
        {
            float xHold = Mathf.Abs(transform.localScale.x);
            transform.localScale = new Vector3(direction.x > 0 ? -xHold : xHold, transform.localScale.y, 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only explode on Player contact
        if (collision.CompareTag("Player") && !isDead)
        {
            Explode();
        }
    }

    // IMPORTANT: This must be called by EnemyHealth.Die() or TakeDamage()
    public void OnDeath()
    {
        if (!isDead) Explode();
    }

    void Explode()
    {
        if (isDead) return;
        isDead = true;
        
        CancelInvoke("Explode"); // Stop the timer

        if(rb != null) 
        {
            rb.linearVelocity = Vector2.zero; 
            rb.simulated = false; // Stop them from pushing Tyrone after death
        }

        if (anim != null) anim.SetTrigger("die"); 

        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Apply knockback to Tyrone
        if (target != null)
        {
            PlayerMovement pMove = target.GetComponent<PlayerMovement>();
            if (pMove != null)
            {
                Vector2 knockbackDir = (target.position - transform.position).normalized;
                pMove.ApplyKnockback(knockbackDir * 18f); // Stronger pop
            }
        }

        Destroy(gameObject, 0.5f); 
    }
}