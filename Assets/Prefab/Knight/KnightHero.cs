using UnityEngine;
using System.Collections;

public class KnightHero : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    
    [Header("Dash Settings")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1.5f;
    private float nextDashTime = 0f;
    private bool isDashing = false;
    
    public float climbSpeed = 6f;
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Jump Settings")]
    public int maxJumps = 2;
    private int jumpCount = 0;

    [Header("Combat Settings")]
    public Transform attackPoint;      
    public float attackRange = 1.25f;   
    public float attackDamage = 25f;   
    public LayerMask enemyLayers;     

    [Header("Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    [Header("Restrictions")]
    public bool canAttack = true; 
    public bool canDash = true;   

    [Header("Companion (Tala)")]
    public Transform talaTransform;
    public Vector3 talaOffset = new Vector3(-1.5f, 1.5f, 0f); 
    public float followSpeed = 5f; 

    [Header("Audio Clips")]
    public AudioClip runSound;
    public AudioClip dashSound;
    public AudioClip[] attackSounds;
    public AudioClip hitSomeoneSound;
    public AudioClip takeDamageSound;
    public float runPitch = 1f;
    public float attackPitchVariance = 0.1f;

    [Header("UI References")]
    public PlayerUI playerUI;
    
    [Header("Mobile Input (Android)")]
    private bool mobileJumpPressed;
    private bool mobileAttackPressed;
    private bool mobileDashPressed;

    // Call these from your UI Buttons Event Triggers (PointerDown)
    public void OnMobileJump() { mobileJumpPressed = true; }
    public void OnMobileAttack() { mobileAttackPressed = true; }
    public void OnMobileDash() { mobileDashPressed = true; }

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private AudioSource audioSource;
    private AudioSource sfxSource;
    private System.Collections.Generic.Dictionary<AudioClip, float> clipOffsets = new System.Collections.Generic.Dictionary<AudioClip, float>();
    private Color originalColor;
    private bool isGrounded;
    private bool isAttacking;
    private float moveInput;
    private float verticalInput;
    private bool canClimb;
    private bool isClimbing;
    private float originalGravity;
    private string currentAnimState; 
    private bool isStunned = false;
    private float stunDuration = 0.4f;

    // Input state latches
    private bool prevGamepadJump;
    private bool prevGamepadAttack;
    private bool prevGamepadDash;

    void Awake() {
        AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include);
        if (listeners.Length > 1) {
            bool keptOne = false;
            foreach (var listener in listeners) {
                if (listener.gameObject.CompareTag("MainCamera") && !keptOne) {
                    listener.enabled = true;
                    keptOne = true;
                } else listener.enabled = false;
            }
        }

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
        
        if (attackPoint == null) {
            Transform found = transform.Find("AttackPoint");
            if (found != null) attackPoint = found;
            else {
                var go = new GameObject("AttackPoint");
                go.transform.SetParent(transform, false);
                attackPoint = go.transform;
            }
        }
        // Force the AttackPoint to chest height (y = 1.1f) for the bottom-pivoted Yves sprite, but preserve custom positions!
        if (attackPoint != null && attackPoint.localPosition == Vector3.zero) {
            attackPoint.localPosition = new Vector3(0.85f, 1.1f, 0f);
        }

        // Upgrade legacy default attackRange to match the new Yves sprite reach
        if (attackRange == 1.25f) {
            attackRange = 2.0f;
            Debug.Log("<color=cyan>KnightHero: Upgraded legacy default attackRange from 1.25 to 2.0 to match the new Yves sprite reach.</color>");
        }

        if (enemyLayers.value == 0) {
            int enemy = LayerMask.NameToLayer("Enemy");
            if (enemy >= 0) enemyLayers = 1 << enemy;
        }
        
        // --- AUTO-FIX FOR FALLING THROUGH VOID ---
        if (groundLayer.value == 0) {
            int ground = LayerMask.NameToLayer("Ground");
            int wall = LayerMask.NameToLayer("Wall");
            int def = LayerMask.NameToLayer("Default");
            
            LayerMask mask = 0;
            if (ground >= 0) mask |= (1 << ground);
            if (wall >= 0) mask |= (1 << wall);
            if (def >= 0) mask |= (1 << def);
            
            groundLayer = mask;
            Debug.Log($"<color=cyan>KnightHero: groundLayer was empty. Auto-assigned to: {LayerMask.LayerToName(ground)}, {LayerMask.LayerToName(wall)}, {LayerMask.LayerToName(def)}</color>");
        }
        if (playerUI == null) playerUI = GetComponentInChildren<PlayerUI>(true);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) {
            PhysicsMaterial2D frictionless = new PhysicsMaterial2D("SlipperyKnight");
            frictionless.friction = 0f; col.sharedMaterial = frictionless;
        }
    }

    void Start() {
        rb.gravityScale = 3f; 
        if (gameObject.tag != "Player") gameObject.tag = "Player";
        originalGravity = rb.gravityScale;
        currentHealth = maxHealth;
        if (playerUI != null) playerUI.Initialize(maxHealth);
        PreCacheSounds();
    }

    private void PreCacheSounds() {
        GetSilenceOffset(runSound); GetSilenceOffset(dashSound);
        GetSilenceOffset(hitSomeoneSound); GetSilenceOffset(takeDamageSound);
        if (attackSounds != null) foreach (var clip in attackSounds) GetSilenceOffset(clip);
    }

    void OnDisable() {
        StopRunSound();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        ChangeAnimationState("KnightIdle");
    }

    void Update() {
        // --- GLOBAL DIALOGUE BLOCKER ---
        // If a dialogue panel is active in the scene, completely freeze the player, stop run sounds, and force Idle state!
        if (LBYH_Dialogue.Instance != null && LBYH_Dialogue.Instance.IsVisible)
        {
            moveInput = 0f;
            verticalInput = 0f;
            isClimbing = false;
            StopRunSound();
            ChangeAnimationState("KnightIdle");
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            return;
        }

        if (isDashing) return;

        // --- INPUT HANDLING ---
        moveInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

#if ENABLE_INPUT_SYSTEM
        // Use Gamepad.all instead of Gamepad.current because OnScreenStick creates a virtual gamepad
        // that isn't always assigned to "current" when using a mouse in the Editor!
        foreach (var gamepad in UnityEngine.InputSystem.Gamepad.all)
        {
            float joyX = gamepad.leftStick.x.ReadValue();
            if (Mathf.Abs(joyX) > 0.1f)
            {
                if (joyX > 0.3f) moveInput = 1f;
                else if (joyX < -0.3f) moveInput = -1f;
                else moveInput = 0f;
                break;
            }
        }
        foreach (var gamepad in UnityEngine.InputSystem.Gamepad.all)
        {
            float joyY = gamepad.leftStick.y.ReadValue();
            if (Mathf.Abs(joyY) > 0.1f)
            {
                verticalInput = joyY;
                break;
            }
        }
#endif

        bool attackInput = Input.GetMouseButtonDown(0) || mobileAttackPressed;
        mobileAttackPressed = false; // consume

        bool dashInput = Input.GetKeyDown(KeyCode.LeftShift) || mobileDashPressed;
        mobileDashPressed = false;

        bool jumpInput = Input.GetButtonDown("Jump") || mobileJumpPressed;
        mobileJumpPressed = false;

#if ENABLE_INPUT_SYSTEM
        bool gamepadJump = false;
        bool gamepadAttack = false;
        bool gamepadDash = false;
        foreach (var gamepad in UnityEngine.InputSystem.Gamepad.all)
        {
            if (gamepad.buttonSouth.isPressed) gamepadJump = true;
            if (gamepad.buttonWest.isPressed) gamepadAttack = true;
            if (gamepad.buttonEast.isPressed) gamepadDash = true;
        }

        // Simulate "GetButtonDown" manually to bypass On-Screen Button frame skipping bugs
        if (gamepadJump && !prevGamepadJump) jumpInput = true;
        if (gamepadAttack && !prevGamepadAttack) attackInput = true;
        if (gamepadDash && !prevGamepadDash) dashInput = true;

        prevGamepadJump = gamepadJump;
        prevGamepadAttack = gamepadAttack;
        prevGamepadDash = gamepadDash;
#endif

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded || isClimbing) jumpCount = 0;

        if (canAttack && attackInput && !isAttacking) StartCoroutine(PerformAttack());
        if (canDash && dashInput && !isDashing && Time.time >= nextDashTime) StartCoroutine(PerformDash());
        
        if (jumpInput && !isAttacking) {
            if (isGrounded || isClimbing) {
                isClimbing = false;
                jumpCount = 1;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                ChangeAnimationState("KnightJump");
            } else if (jumpCount < maxJumps) {
                // Smooth Double Jump: Reset Y velocity so falling doesn't weaken the second jump
                jumpCount++;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                ChangeAnimationState("KnightJump"); // Re-trigger jump animation
                Debug.Log("KnightHero: Double Jump!");
            }
        }

        if (canClimb && Mathf.Abs(verticalInput) > 0.1f) isClimbing = true;
        if (!canClimb || (isGrounded && verticalInput < -0.1f)) isClimbing = false;

        if (!isAttacking && !isDashing) {
            if (isClimbing) ChangeAnimationState("KnightRun"); 
            else if (!isGrounded) ChangeAnimationState("KnightJump");
            else if (Mathf.Abs(moveInput) > 0.1f) { ChangeAnimationState("KnightRun"); PlayRunSound(); }
            else { ChangeAnimationState("KnightIdle"); StopRunSound(); }
        } else StopRunSound();

        if (!isAttacking) {
            if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
            else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);
        }

        if (talaTransform != null && talaTransform.GetComponent<TalaFollow>() == null) {
            Vector3 targetPos = transform.position + new Vector3(talaOffset.x * transform.localScale.x, talaOffset.y, 0);
            talaTransform.position = Vector3.Lerp(talaTransform.position, targetPos, followSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate() {
        // --- GLOBAL DIALOGUE BLOCKER ---
        if (LBYH_Dialogue.Instance != null && LBYH_Dialogue.Instance.IsVisible)
        {
            rb.gravityScale = 3f;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (isDashing || isAttacking || isStunned) return; 
        if (isClimbing) {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, verticalInput * climbSpeed);
        } else {
            rb.gravityScale = 3f;
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    IEnumerator PerformAttack() {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero; 
        ChangeAnimationState("KnightAttack");
        
        if (attackSounds != null && attackSounds.Length > 0) {
            AudioClip randomClip = attackSounds[Random.Range(0, attackSounds.Length)];
            sfxSource.pitch = 1.0f + Random.Range(-attackPitchVariance, attackPitchVariance);
            PlayTrimmedSFX(randomClip);
        }

        yield return new WaitForSeconds(0.28f); // Damage frame perfectly matches the Yves slash frame (0.283s)!
        DealDamage();
        yield return new WaitForSeconds(0.28f); // Complete the 0.56s animation fully before transitioning
        isAttacking = false;
    }

    private void DealDamage() {
        if (attackPoint == null) return;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        
        Debug.Log($"<color=white>Knight Attack: Detected {hitEnemies.Length} objects on Enemy Layer.</color>");

        if (hitEnemies.Length > 0 && hitSomeoneSound != null) {
            PlayTrimmedSFX(hitSomeoneSound);
        }

        foreach (Collider2D enemy in hitEnemies) {
            Debug.Log($"<color=yellow>Knight Hit: {enemy.name}</color>");
            EnemyHealth health = enemy.GetComponent<EnemyHealth>() ?? enemy.GetComponentInParent<EnemyHealth>();
            BossHealth bossHealth = enemy.GetComponent<BossHealth>() ?? enemy.GetComponentInParent<BossHealth>();
            DamonBossController damon = enemy.GetComponent<DamonBossController>() ?? enemy.GetComponentInParent<DamonBossController>();

            if (damon != null) damon.TakeDamage(attackDamage);
            else if (health != null) health.TakeDamage(attackDamage, (enemy.transform.position - transform.position).normalized);
            else if (bossHealth != null) bossHealth.TakeDamage((int)attackDamage);
        }
    }

    IEnumerator PerformDash() {
        isDashing = true;
        if (anim != null && anim.HasState(0, Animator.StringToHash("KnightDash"))) {
            ChangeAnimationState("KnightDash");
        } else {
            ChangeAnimationState("KnightSlide");
        }
        PlayTrimmedSFX(dashSound);
        float oldGrav = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(transform.localScale.x * dashForce, 0);
        yield return new WaitForSeconds(dashDuration);
        rb.gravityScale = oldGrav;
        isDashing = false;
        nextDashTime = Time.time + dashCooldown;
    }

    void ChangeAnimationState(string newState) {
        if (currentAnimState == newState || anim == null) return;
        
        // Safety check: Does the state actually exist?
        if (!anim.HasState(0, Animator.StringToHash(newState))) {
            return; 
        }

        anim.Play(newState);
        currentAnimState = newState;
    }

    public void TakeDamage(float damage) {
        if (currentHealth <= 0 || isDashing) return; // Invulnerable during Dash!

        currentHealth -= damage;
        if (playerUI != null) playerUI.UpdateHealth(currentHealth);
        StartCoroutine(HitFlashEffect());
        ChangeAnimationState("KnightHit"); 
        if (takeDamageSound != null) PlayTrimmedSFX(takeDamageSound);
        if (currentHealth <= 0) Die();
    }

    private IEnumerator HitFlashEffect() {
        if (sr == null) yield break;
        for (int i = 0; i < 3; i++) {
            sr.color = Color.red; yield return new WaitForSeconds(0.1f);
            sr.color = originalColor; yield return new WaitForSeconds(0.1f);
        }
    }

    private void Die() {
        if (playerUI != null) playerUI.ShowDeathMenu();
        else UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private void PlayRunSound() {
        if (runSound == null || audioSource == null) return;
        if (!audioSource.isPlaying || audioSource.clip != runSound) {
            audioSource.clip = runSound; audioSource.loop = true;
            audioSource.time = GetSilenceOffset(runSound); audioSource.Play();
        }
    }

    private void StopRunSound() { if (audioSource != null && audioSource.clip == runSound) audioSource.Stop(); }

    private void PlayTrimmedSFX(AudioClip clip) {
        if (clip == null || sfxSource == null) return;
        sfxSource.Stop(); sfxSource.clip = clip;
        sfxSource.time = GetSilenceOffset(clip); sfxSource.Play();
    }

    private float GetSilenceOffset(AudioClip clip) {
        if (clip == null) return 0f;
        if (clipOffsets.TryGetValue(clip, out float cached)) return cached;
        float threshold = 0.001f;
        int samplesToScan = Mathf.Min(clip.samples, (int)(clip.frequency * 2.0f));
        float[] samples = new float[samplesToScan * clip.channels];
        clip.GetData(samples, 0);
        for (int i = 0; i < samples.Length; i++) {
            if (Mathf.Abs(samples[i]) > threshold) {
                float offset = (float)i / (clip.frequency * clip.channels);
                clipOffsets[clip] = offset; return offset;
            }
        }
        return 0f;
    }

    public void ApplyKnockback(Vector2 force)
    {
        if (rb == null) return;
        isStunned = true;
        rb.linearVelocity = force;
        CancelInvoke(nameof(EndStun));
        Invoke(nameof(EndStun), stunDuration);
    }

    private void EndStun() => isStunned = false;

    private int ladderCount = 0;
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.GetComponent<Ladder>() != null || collision.name.ToLower().Contains("ladder")) {
            ladderCount++; canClimb = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.GetComponent<Ladder>() != null || collision.name.ToLower().Contains("ladder")) {
            ladderCount--; if (ladderCount <= 0) { ladderCount = 0; canClimb = false; isClimbing = false; }
        }
    }
    void OnDrawGizmosSelected() { if (attackPoint != null) { Gizmos.color = Color.red; Gizmos.DrawWireSphere(attackPoint.position, attackRange); } }
}