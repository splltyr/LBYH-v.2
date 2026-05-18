using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamonFinale : MonoBehaviour
{
    [Header("Setup")]
    public Transform player;
    public float moveSpeed = 3f;
    public float attackRange = 5f;
    public float damage = 25f;

    [Header("Health & UI")]
    public float maxHealth = 500f;
    public float currentHealth;
    public Slider healthBar;

    [Header("Animations")]
    public AnimationClip idle;
    public AnimationClip walk;
    public AnimationClip attack;
    public AnimationClip hit;

    private Animator anim;
    private SpriteRenderer sr;
    private bool isAttacking = false;
    private bool isDead = false;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    void Update()
    {
        if (player == null || isAttacking || isDead) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            // WALK
            float dir = (player.position.x > transform.position.x) ? 1 : -1;
            transform.Translate(new Vector3(dir * moveSpeed * Time.deltaTime, 0, 0));
            
            if (anim != null && walk != null) anim.Play(walk.name);
            
            // Fixed Flip for Sprite Renderer
            if (sr != null) sr.flipX = (dir > 0);
        }
        else
        {
            // ATTACK
            StartCoroutine(PerformAttack());
        }
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        if (anim != null && attack != null) anim.Play(attack.name);
        
        yield return new WaitForSeconds(1f); // Wait for hit frame
        
        if (!isDead && Vector2.Distance(transform.position, player.position) <= attackRange + 1.5f)
        {
            player.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }

        yield return new WaitForSeconds(1f); // Cooldown
        isAttacking = false;
        if (!isDead && anim != null && idle != null) anim.Play(idle.name);
    }

    // This function is for compatibility with other scripts
    public void takeDamage(float amount)
    {
        TakeDamage(amount);
    }

    // Overload for compatibility with scripts that send (amount, direction)
    public void TakeDamage(float amount, Vector2 direction)
    {
        TakeDamage(amount);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log("<color=yellow>Boss took damage: " + amount + " | HP: " + currentHealth + "</color>");
        
        if (healthBar != null) healthBar.value = currentHealth;
        
        if (anim != null && hit != null) anim.Play(hit.name);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("<color=red>Damon Finale: DEFEATED!</color>");
        // Trigger whatever final sequence you want here
    }

    public bool IsDefeated()
    {
        return isDead;
    }
}