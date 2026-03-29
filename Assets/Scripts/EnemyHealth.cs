using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 12f;
    [SerializeField] private float knockbackDuration = 0.15f;

    [Header("UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private GameObject healthBarCanvas;
    [SerializeField] private Image fillImage;

    [Header("Death Effects")]
    [SerializeField] private string deathTrigger = "die";
    [SerializeField] private SpriteRenderer spriteRenderer; // Drag Boss Sprite here
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

    public void TakeDamage(float damage, Vector2 knockbackDir)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (healthBar != null) healthBar.value = currentHealth;

        StopAllCoroutines(); 
        StartCoroutine(ApplyKnockback(knockbackDir));
        StartCoroutine(HitFlash());

        if (currentHealth <= 0) Die();
    }

    private IEnumerator HitFlash()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = Color.white; // Flash white on hit
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private IEnumerator ApplyKnockback(Vector2 direction)
    {
        if (rb != null)
        {
            rb.linearVelocity = direction * knockbackForce;
            yield return new WaitForSeconds(knockbackDuration);
            if (!isDead) rb.linearVelocity = Vector2.zero;
        }
    }

    private void Die()
    {
        isDead = true;
        if (anim != null) anim.SetTrigger(deathTrigger);
        
        // Start the Death Flash/Fade effect
        StartCoroutine(DeathEffect());

        BossController boss = GetComponent<BossController>();
        if (boss != null) boss.DisableBoss();

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
            // Flicker between white and transparent
            spriteRenderer.color = (elapsed % 0.2f > 0.1f) ? Color.white : new Color(1, 1, 1, 0);
            yield return null;
        }
    }
    public float GetCurrentHealthPercentage()
{
    return currentHealth / maxHealth;
}
}