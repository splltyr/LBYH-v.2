using UnityEngine;
using System.Collections;

public class DamonSlimeBoss : MonoBehaviour
{
    [Header("Targeting")]
    public Transform player; 

    [Header("Stats")]
    public float maxHealth = 500f;
    public float moveSpeed = 2f;
    public float attackRange = 5f;
    public float attackCooldown = 3f;
    public float damage = 30f;

    [Header("Animation Assets")]
    public AnimationClip idleClip;
    public AnimationClip walkClip;
    public AnimationClip attackClip;
    public AnimationClip hitClip;

    private EnemyHealth health;
    private Animator anim;
    private SpriteRenderer sr;
    private Color originalColor;
    private bool isAttacking = false;
    private bool isReady = false;
    private float lastAttackTime;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        if (health != null)
        {
            health.maxHealth = maxHealth;
            health.currentHealth = maxHealth;
        }

        // REMOVE RIGIDBODY DEPENDENCY FOR MOVEMENT
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) 
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
        }
    }

    public void StartBossFight()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        isReady = true;
    }

    void Update()
    {
        if (!isReady || player == null || isAttacking) return;

        float distance = Mathf.Abs(transform.position.x - player.position.x);

        if (distance <= attackRange && Time.time > lastAttackTime + attackCooldown)
        {
            StartCoroutine(SimpleAttack());
        }
        else
        {
            SimpleMove();
        }
    }

    void SimpleMove()
    {
        float targetX = player.position.x;
        float direction = (targetX > transform.position.x) ? 1 : -1;
        
        // Use transform.Translate with a fixed speed to ELIMINATE JITTER
        // This is the most stable way to move without physics interference
        float moveStep = direction * moveSpeed * Time.deltaTime;
        transform.Translate(new Vector3(moveStep, 0, 0), Space.World);

        if (anim != null && walkClip != null) anim.Play(walkClip.name);

        // Flip
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * (direction > 0 ? 1 : -1), transform.localScale.y, 1);
    }

    IEnumerator SimpleAttack()
    {
        isAttacking = true;
        if (anim != null && attackClip != null) anim.Play(attackClip.name);
        
        yield return new WaitForSeconds(0.6f);

        if (player != null && Mathf.Abs(transform.position.x - player.position.x) <= attackRange + 1f)
        {
            player.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }

        yield return new WaitForSeconds(1.5f);
        lastAttackTime = Time.time;
        isAttacking = false;
    }

    public void OnDamaged()
    {
        StartCoroutine(FlashRed());
    }

    IEnumerator FlashRed()
    {
        if (sr == null) yield break;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        sr.color = originalColor;
    }

    public bool IsDefeated()
    {
        return (health != null && health.currentHealth <= 0);
    }
}
