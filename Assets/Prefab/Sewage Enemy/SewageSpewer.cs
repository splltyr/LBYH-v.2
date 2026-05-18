using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SewageSpewer : MonoBehaviour
{
    [Header("Manual Setup")]
    public Transform player; 
    public Slider healthBar;
    public float maxHealth = 100f;

    [Header("Combat Setup")]
    public GameObject fireballPrefab;
    public Transform shootPoint;
    public float fireRate = 2f;
    public float detectRange = 25f; // Increased range
    public float damage = 15f;
    
    private Animator anim;
    private float nextFireTime;
    private float currentHealth;
    private bool isDead = false;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;
        
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (anim != null) anim.Play("idle");
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Face the player
        float dir = (player.position.x > transform.position.x) ? -1 : 1;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * dir, transform.localScale.y, 1);

        // Shoot logic - Continuous loop
        if (distance <= detectRange && Time.time >= nextFireTime)
        {
            SpewFireball();
            nextFireTime = Time.time + fireRate;
        }
    }

    void SpewFireball()
    {
        if (isDead) return;

        // Play animation (triggers independently)
        if (anim != null) anim.Play("attack");

        // Spawn fireball
        if (fireballPrefab != null && shootPoint != null)
        {
            GameObject ball = Instantiate(fireballPrefab, shootPoint.position, Quaternion.identity);
            
            // Aim for player chest
            Vector3 targetPos = player.position + Vector3.up;
            Vector3 shootDir = (targetPos - shootPoint.position).normalized;
            
            SewerProjectile projectile = ball.GetComponent<SewerProjectile>();
            if (projectile != null) 
            {
                projectile.Setup(shootDir);
                projectile.damage = damage;
            }
        }
        
        // Safety return to idle after a short delay
        Invoke("ReturnToIdle", 0.5f);
    }

    void ReturnToIdle()
    {
        if (!isDead && anim != null) anim.Play("idle");
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        if (healthBar != null) healthBar.value = currentHealth;
        if (currentHealth <= 0) Die();
    }

    public void TakeDamage(float amount, Vector2 direction)
    {
        TakeDamage(amount);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        CancelInvoke();
        if (anim != null) anim.Play("death");
        Destroy(gameObject, 1f);
    }
}
