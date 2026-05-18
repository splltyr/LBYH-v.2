using UnityEngine;
using System.Collections;

public class DamonBossController : MonoBehaviour
{
    [Header("SCENE 10 VARIANT (BOSS ONLY)")]
    [Header("Stats")]
    public float maxHealth = 200f;
    public float currentHealth;
    public float phase2Threshold = 30f;

    [Header("Phase Settings")]
    public GameObject executionerPrefab;
    public GameObject spawnEffectPrefab; // Visual effect for spawning
    public Transform[] spawnPoints;
    public Transform enemyContainer; // Container to parent spawned enemies to
    public GameObject bigBossPrefab; 
    public GameObject transformationEffect; 
    
    [Header("Combat Settings")]
    public float moveSpeed = 3f;
    public float attackRange = 4f;
    public float attackDamage = 25f;
    public float timeBetweenAttacks = 3.5f;
    public float windUpTime = 0.7f;
    public GameObject projectilePrefab; // For ranged attacks
    public float summonChance = 0.3f; // 30% chance to summon instead of melee
    
    [Header("Animation Settings")]
    public string attackTrigger = "Attack 1";
    public string summonTrigger = "Attack 1";
    public string walkBool = "DamonWalk";
    public string hurtTrigger = "DamonHit";
    
    [Header("Levitation Effect")]
    public bool useLevitation = true; // Toggle for levitation (Human only)
    public float levitationAmplitude = 0.5f;
    public float levitationFrequency = 2f;
    private Vector3 spriteOriginPos;

    [SerializeField] public Transform player; // Manual slot for the player
    private EnemyHealth healthComponent;
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Color originalColor;
    
    private bool isPhase2 = false;
    private bool isInvulnerable = false;
    private bool isAttacking = false;
    private float lastAttackTime;

    void Awake()
    {
        healthComponent = GetComponent<EnemyHealth>();
        if (healthComponent == null) healthComponent = gameObject.AddComponent<EnemyHealth>();
        
        healthComponent.maxHealth = maxHealth;
        currentHealth = maxHealth;
        
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) 
        {
            originalColor = sr.color;
            spriteOriginPos = sr.transform.localPosition;
        }
        
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Apply Levitation (Bobbing) - Always active even if invulnerable!
        if (useLevitation && sr != null)
        {
            float newY = spriteOriginPos.y + Mathf.Sin(Time.time * levitationFrequency) * levitationAmplitude;
            sr.transform.localPosition = new Vector3(spriteOriginPos.x, newY, spriteOriginPos.z);
        }
        else if (!useLevitation && sr != null)
        {
            sr.transform.localPosition = spriteOriginPos;
        }

        // --- COMBAT LOCK ---
        if (isInvulnerable || player == null) 
        {
            if (rb != null) rb.linearVelocity = Vector2.zero; // STOP GLIDING!
            return;
        }

        // Sync HP from EnemyHealth
        if (healthComponent != null)
        {
            currentHealth = healthComponent.currentHealth;
            
            // Safety: Lock HP at 30 until the SequenceController triggers the transformation
            if (!isPhase2 && currentHealth < phase2Threshold)
            {
                currentHealth = phase2Threshold;
                healthComponent.currentHealth = phase2Threshold;
            }
        }

        // Basic AI
        float distance = Vector2.Distance(transform.position, player.position);
        if (!isAttacking)
        {
            if (distance <= attackRange)
            {
                if (Time.time >= lastAttackTime + timeBetweenAttacks)
                {
                    StartCoroutine(PerformAttack());
                }
            }
            else
            {
                MoveTowardsPlayer();
            }
        }
        
        // Face player
        if (!isAttacking)
        {
            float dir = (player.position.x > transform.position.x) ? 1 : -1;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * dir, transform.localScale.y, 1);
        }
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        if (anim != null) anim.SetBool("isWalking", false);
        
        float roll = Random.value;
        
        if (roll < summonChance)
        {
            // --- SUMMON ATTACK (Spawns Executioner Ads) ---
            if (anim != null && !string.IsNullOrEmpty(summonTrigger)) anim.SetTrigger(summonTrigger);
            yield return new WaitForSeconds(windUpTime);
            SpawnExecutioners(1);
        }
        else
        {
            // Melee and projectile attacks are removed from this script so the player
            // doesn't take damage. The custom fireball component will handle attacks.
            yield return null;
        }

        yield return new WaitForSeconds(1.2f); // Recovery
        lastAttackTime = Time.time;
        isAttacking = false;
    }

    private void ShootProjectile()
    {
        // Ranged attacks are now handled by your custom fireball script in Unity!
    }

    private void MoveTowardsPlayer()
    {
        if (player == null || rb == null) return;
        
        Vector2 direction = (player.position - transform.position).normalized;
        
        if (useLevitation)
        {
            // Move on both X and Y (Flying/Levitating)
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            // Move only on X (Grounded Giant)
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        }
        
        // Flip sprite to face direction of movement
        if (direction.x != 0)
        {
            float dir = direction.x > 0 ? 1 : -1;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * dir, transform.localScale.y, 1);
        }
        
        if (anim != null && !string.IsNullOrEmpty(walkBool)) anim.SetBool(walkBool, true);
    }

    // --- DAMAGE SYSTEM ---
    public void TakeDamage(float damage)
    {
        if (isInvulnerable)
        {
            Debug.Log("<color=yellow>Damon: Hit blocked (Invulnerable)!</color>");
            return;
        }

        if (healthComponent != null)
        {
            healthComponent.TakeDamage(damage, Vector2.zero);
            if (anim != null && !string.IsNullOrEmpty(hurtTrigger)) anim.SetTrigger(hurtTrigger);
            Debug.Log($"<color=red>Damon: Took {damage} damage. HP: {healthComponent.currentHealth}</color>");
        }
    }

    public void SetInvulnerable(bool state)
    {
        isInvulnerable = state;
        if (healthComponent != null) healthComponent.enabled = !state;
    }

    public float GetCurrentHP()
    {
        if (healthComponent != null) return healthComponent.currentHealth;
        return currentHealth;
    }

    public void StartPhase1()
    {
        Debug.Log("Damon Phase 1: Spawning 2 Executioners");
        SpawnExecutioners(2);
    }

    private bool hasStartedFight = false; // Safety flag for IsDefeated

    public void StartPhase2()
    {
        isPhase2 = true;
        hasStartedFight = true; // BOSS IS NOW READY TO FIGHT
        // Ensure health is full when starting Phase 2
        if (healthComponent != null) 
        {
            healthComponent.currentHealth = maxHealth;
            currentHealth = maxHealth;
        }
        Debug.Log("Damon Phase 2: Transformation Complete. Spawning 5 Executioners");
        SpawnExecutioners(5);
    }

    public GameObject TransformToBigBoss()
    {
        if (transformationEffect != null)
            Instantiate(transformationEffect, transform.position, Quaternion.identity);

        if (bigBossPrefab != null)
        {
            GameObject bigBoss = Instantiate(bigBossPrefab, transform.position, transform.rotation);
            gameObject.SetActive(false);
            return bigBoss;
        }

        // Fallback
        isPhase2 = true; 
        transform.localScale *= 2.5f; 
        return gameObject;
    }

    public bool IsDefeated()
    {
        if (!hasStartedFight) return false; // Can't be defeated if fight hasn't started!
        if (healthComponent == null) return false;
        return healthComponent.currentHealth <= 0;
    }

    private void SpawnExecutioners(int count)
    {
        if (executionerPrefab == null || spawnPoints == null || spawnPoints.Length == 0) return;
        
        for (int i = 0; i < count; i++)
        {
            int pointIndex = i % spawnPoints.Length;
            Vector3 spawnPos = spawnPoints[pointIndex].position;
            
            // 1. Spawn Effect
            if (spawnEffectPrefab != null)
            {
                Instantiate(spawnEffectPrefab, spawnPos, Quaternion.identity);
            }
            
            // 2. Spawn Executioner
            GameObject exe = Instantiate(executionerPrefab, spawnPos, Quaternion.identity);
            if (enemyContainer != null) exe.transform.SetParent(enemyContainer);
            
            // 3. Ensure it's in Combat mode if it uses BossController
            BossController bc = exe.GetComponent<BossController>();
            if (bc != null) 
            {
                bc.mode = BossController.BossMode.CombatBoss;
                bc.player = player; // Ensure it has a target
            }
        }
    }
}
