using UnityEngine;
using System.Collections;
using TMPro;

public class CorruptedCooker : MonoBehaviour
{
    [Header("Targeting & Detection")]
    public Transform playerTransform; 
    public float activationRange = 8f; 

    [Header("UI Reference")]
    public GameObject dialogueBox;     
    public TextMeshProUGUI textDisplay; 
    public TextMeshProUGUI nameDisplay; 
    public float typeSpeed = 0.04f;

    [Header("Dialogue Content")]
    [TextArea(3, 10)] public string[] introDialogue; 

    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public float chaseRange = 15f;
    public float attackRange = 2.2f;

    [Header("Combat Stats")]
    public float attackDamage = 30f;
    public float attackCooldown = 3f; 
    private float nextAttackTime;

    [Header("Phase 2: Skeleton Reinforcements")]
    public GameObject skeletonPrefab; 
    public Transform[] spawnPoints;   
    private bool hasSpawnedSkeletons = false;

    private Animator anim;
    private Rigidbody2D rb;
    private EnemyHealth health;
    private SpriteRenderer spriteRenderer; 
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isTalking = false;
    private bool isTyping = false;
    private bool hasStartedDialogue = false; 
    private int dialogueIndex = 0;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (rb != null) rb.freezeRotation = true;
    }

    void Update()
    {
        if (isDead) return;

        // 1. DETECTION RANGE CHECK
        if (!hasStartedDialogue)
        {
            float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            Scene3Manager manager = FindAnyObjectByType<Scene3Manager>();
            
            if (distToPlayer <= activationRange && manager != null && manager.currentState == Scene3Manager.SceneState.HuntCooker)
            {
                hasStartedDialogue = true;
                StartCoroutine(StartBossIntro());
            }
            return; 
        }

        if (playerTransform == null || isAttacking || isTalking) return; 
        
        // 2. HP CHECK: SPAWN SKELETONS AT 50% HP
        if (health != null)
        {
            if (health.currentHealth <= 50f && !hasSpawnedSkeletons)
            {
                StartCoroutine(SpawnSkeletonsPhase());
                return;
            }
            if (health.currentHealth <= 0) { Die(); return; }
        }

        // 3. COMBAT MOVEMENT & ATTACK
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        
        if (distance <= attackRange)
        {
            if (Time.time >= nextAttackTime) StartAttack();
            else StopMoving(); 
        }
        else if (distance <= chaseRange) MoveToPlayer();
        else StopMoving();
    }

    IEnumerator StartBossIntro()
    {
        isTalking = true;
        if (playerTransform != null) playerTransform.GetComponent<KnightHero>().enabled = false;
        dialogueBox.SetActive(true);
        
        while (dialogueIndex < introDialogue.Length)
        {
            yield return StartCoroutine(TypeSentence(introDialogue[dialogueIndex]));
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E));
            yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.E));
            dialogueIndex++;
        }

        dialogueBox.SetActive(false);
        if (playerTransform != null) playerTransform.GetComponent<KnightHero>().enabled = true;
        isTalking = false; 
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        textDisplay.text = "";
        
        if (sentence.Contains("RATIONS") || sentence.Contains("WHO DARES")) nameDisplay.text = "Corrupted Cooker";
        else if (sentence.Contains("Someone") || sentence.Contains("crazier")) nameDisplay.text = "Tala";
        else nameDisplay.text = "Yves";

        foreach (char letter in sentence.ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }
        isTyping = false;
    }

    IEnumerator SpawnSkeletonsPhase()
    {
        hasSpawnedSkeletons = true;
        isAttacking = true; 
        StopMoving();
        if (anim != null) anim.SetTrigger("attack");

        foreach (Transform spot in spawnPoints)
        {
            if (skeletonPrefab != null && spot != null)
            {
                Instantiate(skeletonPrefab, spot.position, Quaternion.identity);
                yield return new WaitForSeconds(0.4f);
            }
        }
        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }

    void MoveToPlayer()
    {
        float directionX = playerTransform.position.x - transform.position.x;
        transform.localScale = new Vector3(directionX > 0 ? -1 : 1, 1, 1);
        if (anim != null) anim.SetBool("isRunning", true);
        rb.linearVelocity = new Vector2((directionX > 0 ? 1 : -1) * moveSpeed, rb.linearVelocity.y);
    }

    void StopMoving()
    {
        if (anim != null) anim.SetBool("isRunning", false);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void StartAttack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;
        StopMoving();
        if (anim != null) anim.SetTrigger("attack");
    }

    public void ResetAttack() { isAttacking = false; }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        StopMoving();
        if (anim != null) anim.SetTrigger("isDead");
        GetComponent<Collider2D>().enabled = false;
        rb.simulated = false;
        StartCoroutine(DeathFlicker());
    }

    IEnumerator DeathFlicker()
    {
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            if (spriteRenderer != null) spriteRenderer.enabled = !spriteRenderer.enabled; // Flash effect
            yield return new WaitForSeconds(0.05f);
        }
        Destroy(gameObject);
    }
}