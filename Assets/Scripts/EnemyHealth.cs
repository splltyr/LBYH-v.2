using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    public float currentHealth;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 12f;
    [SerializeField] private float knockbackDuration = 0.15f;

    [Header("UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private GameObject healthBarCanvas;
    [SerializeField] private Image fillImage;

    [Header("Death Effects")]
    [SerializeField] private string deathTrigger = "die";
    [SerializeField] private SpriteRenderer spriteRenderer; 
    private Color originalColor;
    private bool isDead = false;

    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = maxHealth;
        }
    }

    public void TakeDamage(float damage, Vector2 knockbackDirection)
    {
        if (isDead) return;

        currentHealth -= damage;
        UpdateHealthBar(); // Fixed name mismatch (Capital U)

        // Trigger Visual Effects
        StartCoroutine(HitFlash());
        if (knockbackDirection != Vector2.zero)
        {
            StartCoroutine(ApplyKnockback(knockbackDirection));
        }

        // Trigger specific AI Hit animations
        SkeletonEnemy skel = GetComponent<SkeletonEnemy>();
        if (skel != null) skel.OnHit();

        if (currentHealth <= 0) Die();
    }

    // Function added to fix the CS0103 error
    public void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
    }

    private IEnumerator HitFlash()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = Color.white; 
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private IEnumerator ApplyKnockback(Vector2 direction)
    {
        if (rb != null)
        {
            // Unity 6 uses linearVelocity instead of velocity
            rb.linearVelocity = direction.normalized * knockbackForce;
            yield return new WaitForSeconds(knockbackDuration);
            if (!isDead) rb.linearVelocity = Vector2.zero;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (anim != null) anim.SetTrigger(deathTrigger);
        
        StartCoroutine(DeathEffect());

        // Check for Boss specific scripts
        BossController boss = GetComponent<BossController>();
        if (boss != null) boss.DisableBoss();

        // Disable physics/UI
        GetComponent<Collider2D>().enabled = false;
        if (rb != null) rb.simulated = false;
        if (healthBarCanvas != null) healthBarCanvas.SetActive(false);

        Destroy(gameObject, 2f); 
    }

    private IEnumerator DeathEffect()
    {
        if (spriteRenderer == null) yield break;

        float elapsed = 0f;
        float duration = 1.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // Flicker effect
            spriteRenderer.color = (elapsed % 0.2f > 0.1f) ? Color.white : new Color(1, 1, 1, 0);
            yield return null;
        }
    }

    public float GetCurrentHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}