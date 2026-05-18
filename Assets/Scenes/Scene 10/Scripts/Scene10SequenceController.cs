using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Scene10SequenceController : MonoBehaviour
{
    [Header("Dialogue Content (Editable in Inspector)")]
    [SerializeField] private LBYH_Line[] introLines = new LBYH_Line[] {
        new LBYH_Line { name = "Damon", text = "Look who we have here. My little assistant." },
        new LBYH_Line { name = "Damon", text = "I can smell rats, thousands of miles away." },
        new LBYH_Line { name = "Tala", text = "I am not your assistant." },
        new LBYH_Line { name = "Damon", text = "Sure, stay mad. You wouldn’t have the chance to do it again after I delete you both anyway." },
        new LBYH_Line { name = "Yves", text = "Are you sure you’ll delete us? Or maybe we’ll delete you." },
        new LBYH_Line { name = "Damon", text = "I’d like to see you do that." }
    };

    [SerializeField] private LBYH_Line[] damonDefeatedLines = new LBYH_Line[] {
        new LBYH_Line { name = "Tala", text = "It’s over now, Damon." },
        new LBYH_Line { name = "Damon", text = "Where did it all go wrong, Tala?" },
        new LBYH_Line { name = "Tala", text = "You’re really out of your mind, aren’t you, Damon? You had every chance you could get when I tried stopping you 10 years ago." },
        new LBYH_Line { name = "Damon", text = "I-I don’t remember.." },
        new LBYH_Line { name = "Tala", text = "Of course, you don’t." },
        new LBYH_Line { name = "Damon", text = "This might be the 'losing to you' talking, but I enjoyed our time together, Tala. I should have listened to you rather than going through with the path I am on right now." },
        new LBYH_Line { name = "Damon", text = "You don’t know this, but I was actually observing you the past few years. You helped NPCs that aren’t even real people with empathy.. You really are a great person." },
        new LBYH_Line { name = "Damon", text = "I’m sorry.." },
        new LBYH_Line { name = "Tala", text = "You don’t look like you are." },
        new LBYH_Line { name = "Damon", text = "Tala, please. I know you won’t forgive me but believe me when I say that I regret every inch of what I’ve done to you." },
        new LBYH_Line { name = "Tala", text = "I’ll.. forgive you o-" },
        new LBYH_Line { name = "Damon", text = "What..? NO. You don’t have to force yourself to forgive me.. I don’t deserve that." },
        new LBYH_Line { name = "Tala", text = "Can you let me finish first? I’ll only forgive you, only if you grant 3 of my wishes." },
        new LBYH_Line { name = "Damon", text = "Anything for you, Tala." },
        new LBYH_Line { name = "Tala", text = "First wish; I want you to open the quantum computer in order for us to inject the code, and then delete this world you’ve built." },
        new LBYH_Line { name = "Tala", text = "Second wish; I want Yves to return back to his world." },
        new LBYH_Line { name = "Tala", text = "And Lastly.. I want you to free 'us' both and restart again." },
        new LBYH_Line { name = "Narrative", text = "[Damon took a few seconds to respond because of the shock he’s feeling.]" },
        new LBYH_Line { name = "Damon", text = "Granted." },
        new LBYH_Line { name = "Damon", text = "I guess this is it, huh? Goodbye Tala." },
        new LBYH_Line { name = "Damon", text = "And well played, Yves. Good luck on your journey in real life. May you learn something from this." },
        new LBYH_Line { name = "Damon", text = "I’ll wait for you, Tala." },
        new LBYH_Line { name = "Narrative", text = "(Damon Disappears, with the world shaking)" },
        new LBYH_Line { name = "Tala", text = "I have served my purpose, Yves. You’re free now." },
        new LBYH_Line { name = "Yves", text = "Aren’t you going to come back to the original timeline?" },
        new LBYH_Line { name = "Tala", text = "I wish I could, Yves.." },
        new LBYH_Line { name = "Narrative", text = "[After giving it a thought, Tala made a decision.]" },
        new LBYH_Line { name = "Tala", text = "What do you say about getting your 'keen' eye fixed?" },
        new LBYH_Line { name = "Yves", text = "What do you mean? I do have kee-" },
        new LBYH_Line { name = "Narrative", text = "[Tala then possessed Yves’s right eye]" },
        new LBYH_Line { name = "Yves", text = "AGH! Tala?! Where are you..?" },
        new LBYH_Line { name = "Tala", text = "I’m here, silly. Look at the puddle." },
        new LBYH_Line { name = "Yves", text = "WOAH, MY EYE!" },
        new LBYH_Line { name = "Tala", text = "Pretty cool, right?" },
        new LBYH_Line { name = "Yves", text = "This way, I can remember you even if.. Even if you’re gone." },
        new LBYH_Line { name = "Tala", text = "*giggles*" },
        new LBYH_Line { name = "Yves", text = "Damon must be waiting for you, Tala. You should.. Definitely go now." },
        new LBYH_Line { name = "Tala", text = "Thank you for having me. I enjoyed my time with you." },
        new LBYH_Line { name = "Yves", text = "I’ll miss you, glowy bubble chatter." },
        new LBYH_Line { name = "Tala", text = "Farewell, Yves." },
        new LBYH_Line { name = "Yves", text = "I wish you two a happy ending." }
    };

    [SerializeField] private LBYH_Line[] classroomLines = new LBYH_Line[] {
        new LBYH_Line { name = "Narrative", text = "[A Professor notices Yves sleeping while he’s discussing.]" },
        new LBYH_Line { name = "Professor", text = "I swear, students these days.. Yves, are you sle-" },
        new LBYH_Line { name = "Yves", text = "I-I wasn't sleeping sir!" },
        new LBYH_Line { name = "Professor", text = "Oh really? What were you doing then?" },
        new LBYH_Line { name = "Yves", text = "I uh.. I closed my eyes because your voice is.. It is really charming that uhm.. That it made me feel sleepy.. ehe" },
        new LBYH_Line { name = "Professor", text = "Oh.. Uhm.. *clears throat* Thank you, Yves. That is sweet of you. Now class th-" },
        new LBYH_Line { name = "Yves", text = "(Smirks) Thank you, Tala." }
    };

    [SerializeField] private LBYH_Line[] postCreditsLines = new LBYH_Line[] {
        new LBYH_Line { name = "Yves", text = "I haven’t been to the library for a while. I gotta make my powerpoint presentation." },
        new LBYH_Line { name = "Yves", text = "What is this old CD? I should insert it onto a computer." },
        new LBYH_Line { name = "Yves", text = "WHAT THE-" }
    };

    [SerializeField] private LBYH_Line[] defeatDialogue = new LBYH_Line[] {
        new LBYH_Line { name = "Damon", text = "Well, I guess you defeated me. Was it easy?" },
        new LBYH_Line { name = "Damon", text = "Don't worry, we're just getting started." }
    };

    [Header("Characters & Boss")]
    [SerializeField] private DamonBossController damonBoss;
    [SerializeField] private GameObject damonFinal; // Dedicated slot for damonfinal
    public GameObject finalBossObject; // Retained as fallback for compatibility
    [SerializeField] private GameObject executionerPrefab;
    [SerializeField] private Transform allEnemiesContainer; // Parent of all minions
    [SerializeField] private Transform[] spawnPoints;
    
    [Header("BGM & SFX")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioClip introBGM;
    [SerializeField] private AudioClip bossBGM;
    [SerializeField] private AudioClip endingBGM;
    [SerializeField] private AudioClip classroomBGM;
    [SerializeField] private float musicFadeSpeed = 1.5f;

    [Header("UI")]
    [SerializeField] private LBYH_Dialogue dialogueUI;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private string creditsSceneName = ""; // Leave blank to use local panel, or type new scene name to load it

    [Header("Cinematic Visuals")]
    [SerializeField] private CanvasGroup blackScreen;
    [SerializeField] private GameObject classroomSet;
    [SerializeField] private GameObject puddleEyeReveal;
    
    [Header("Arena & Teleport")]
    [SerializeField] private Transform bossArenaTeleportPoint; // The new arena
    
    [ContextMenu("Force Phase 2 Teleport")]
    public void ForcePhase2Teleport()
    {
        StopAllCoroutines();
        StartCoroutine(SkipToPhase2());
    }

    private IEnumerator SkipToPhase2()
    {
        yield return StartCoroutine(TeleportToFinalArena());
        GameObject activeFinalBoss = damonFinal != null ? damonFinal : finalBossObject;
        if (activeFinalBoss != null)
        {
            activeFinalBoss.SetActive(true);
            DamonSlimeBoss slimeBoss = activeFinalBoss.GetComponent<DamonSlimeBoss>();
            if (slimeBoss != null) slimeBoss.StartBossFight();
            if (player != null) player.enabled = true;
        }
    }

    [ContextMenu("Initialize Dialogue Text")]
    // Overload for compatibility with scripts that send (amount, direction)
    public void TakeDamage(float amount, Vector2 direction)
    {
        TakeDamage(amount);
    }

    public void TakeDamage(float amount)
    {
        // This is mainly for consistency across scenes and to allow quick refresh
        // The text is already set in the Inspector defaults, but this ensures a clean slate
        Debug.Log("Scene 10 Dialogue is already pre-filled in the script arrays. You can assign AudioClips in the Inspector.");
    }

    [Header("Death Settings")]
    public Slider playerHealthBar;
    public GameObject deathPanel;

    private KnightHero player;
    private bool playerDead = false;
    private Coroutine bossPulseCoroutine;

    void Update()
    {
        // Safety check for player death in Scene 10
        if (!playerDead && playerHealthBar != null && playerHealthBar.value <= 0.01f)
        {
            TriggerGameOver();
        }
    }

    void TriggerGameOver()
    {
        playerDead = true;
        if (deathPanel != null) deathPanel.SetActive(true);
        
        // Show mouse so you can click!
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Instead of pausing everything, just stop the player and enemies
        if (player != null) player.enabled = false;
        
        // Find all enemies and disable them
        MonoBehaviour[] allScripts = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude);
        foreach (var s in allScripts)
        {
            if (s is DamonFinale || s is DamonBossController) s.enabled = false;
        }

        Debug.Log("<color=red>Game Over! Cursor enabled for restart.</color>");
    }

    void Start()
    {
        // Safety: Stop any stray AudioSources (like 'Play On Awake' ones on prefabs)
        AudioSource[] allAudio = FindObjectsByType<AudioSource>();
        foreach (var source in allAudio)
        {
            if (source != bgmSource && source != voiceSource) source.Stop();
        }

        // AUTO-FIND EVERYTHING
        // AUTO-FIND EVERYTHING (Including Inactive)
        if (player == null) player = FindAnyObjectByType<KnightHero>(FindObjectsInactive.Include);
        if (damonBoss == null) damonBoss = FindAnyObjectByType<DamonBossController>(FindObjectsInactive.Include);
        if (dialogueUI == null) dialogueUI = FindAnyObjectByType<LBYH_Dialogue>(FindObjectsInactive.Include);
        if (blackScreen == null) blackScreen = GetComponentInChildren<CanvasGroup>();
        
        if (voiceSource == null) voiceSource = gameObject.AddComponent<AudioSource>();
        if (bgmSource == null) 
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
        }

        // Ensure visuals are correct
        if (damonBoss != null) damonBoss.gameObject.SetActive(true);
        if (classroomSet != null) classroomSet.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (puddleEyeReveal != null) puddleEyeReveal.SetActive(false);
        if (blackScreen != null) blackScreen.alpha = 0;

        StartCoroutine(RunFinalConflict());
    }

    IEnumerator RunFinalConflict()
    {
        // 1. THE ARRIVAL
        StartCoroutine(FadeBGM(introBGM));
        if (player != null) 
        {
            player.enabled = false;
            Rigidbody2D pRb = player.GetComponent<Rigidbody2D>();
            if (pRb != null) pRb.linearVelocity = Vector2.zero; // STOP SLIDING
        }
        yield return new WaitForSeconds(1f);
        
        if (damonBoss != null) 
        {
            damonBoss.gameObject.SetActive(true);
            damonBoss.SetInvulnerable(true); 
            Debug.Log("<color=green>Scene 10: Damon enabled and ready for dialogue.</color>");
        }
        else
        {
            Debug.LogError("Scene 10: DamonBossController NOT FOUND! The fight cannot continue.");
        }

        // Focus camera on Damon for the intro
        CameraFollow cam = FindAnyObjectByType<CameraFollow>();
        if (cam != null) cam.target = damonBoss.transform;

        yield return PlayDialogue(introLines);

        // 2. BOSS FIGHT: PHASE 1
        // Return camera to player
        if (cam != null && player != null) cam.target = player.transform;
        
        StartCoroutine(FadeBGM(bossBGM));
        if (player != null) player.enabled = true;
        
        // --- PHASE 1: MINIONS ---
        damonBoss.SetInvulnerable(true); 
        damonBoss.StartPhase1(); 

        // Wait until all enemies in the container are defeated
        if (allEnemiesContainer != null)
        {
            Debug.Log("Scene 10: Waiting for minions to be defeated...");
            yield return new WaitUntil(() => {
                EnemyHealth[] enemies = allEnemiesContainer.GetComponentsInChildren<EnemyHealth>();
                foreach (var enemy in enemies) {
                    if (enemy.currentHealth > 0) return false;
                }
                return true;
            });
            Debug.Log("Scene 10: All minions defeated! Damon is now vulnerable.");
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }

        damonBoss.SetInvulnerable(false);
        
        Debug.Log("<color=cyan>Scene 10: Phase 1 Combat Active. Waiting for 30 HP...</color>");
        while (damonBoss != null && damonBoss.GetCurrentHP() > 30.1f)
        {
            // Debug Log every 2 seconds to show current HP
            if (Time.frameCount % 120 == 0) 
                Debug.Log($"<color=white>Damon HP: {damonBoss.GetCurrentHP()}</color>");
            yield return null;
        }
        
        Debug.Log("<color=orange>Scene 10: Human form defeated! Starting Transition.</color>");

        // 3. THE DEFEAT DIALOGUE & TELEPORT
        if (player != null) 
        {
            player.enabled = false;
            Rigidbody2D pRb = player.GetComponent<Rigidbody2D>();
            if (pRb != null) pRb.linearVelocity = Vector2.zero; // STOP SLIDING AGAIN
        }
        damonBoss.SetInvulnerable(true);
        
        yield return PlayDialogue(defeatDialogue);

        // Transition to the new arena
        yield return StartCoroutine(TeleportToFinalArena());

        // 4. BOSS FIGHT: PHASE 2 (BIG BOSS / SLIME)
        GameObject activeFinalBoss = damonFinal != null ? damonFinal : finalBossObject;
        
        if (activeFinalBoss != null)
        {
            activeFinalBoss.SetActive(true);
            
            // Retrieve any potential boss components
            DamonSlimeBoss slimeBoss = activeFinalBoss.GetComponent<DamonSlimeBoss>();
            DamonFinale finaleBoss = activeFinalBoss.GetComponent<DamonFinale>();
            EnemyHealth eh = activeFinalBoss.GetComponent<EnemyHealth>();
            
            // Explicitly force health initialization so it doesn't start at 0 before Unity lifecycle runs!
            if (eh != null)
            {
                eh.maxHealth = eh.maxHealth > 0 ? eh.maxHealth : 500f;
                eh.currentHealth = eh.maxHealth;
            }
            if (finaleBoss != null)
            {
                finaleBoss.currentHealth = finaleBoss.maxHealth > 0 ? finaleBoss.maxHealth : 500f;
            }

            // Ensure the camera follows the new position immediately
            CameraFollow camFollow = FindAnyObjectByType<CameraFollow>();
            if (camFollow != null) 
            {
                camFollow.useBoundaries = false; // Disable limits so it can reach the 2nd map!
                camFollow.target = player.transform;
            }

            if (slimeBoss != null)
            {
                slimeBoss.StartBossFight();
            }
            
            if (player != null) 
            {
                player.enabled = true;
                // Force player out of any stuck animations
                Animator pAnim = player.GetComponentInChildren<Animator>();
                if (pAnim != null) pAnim.Play("KnightIdle"); 
            }
            
            Debug.Log("<color=cyan>Scene 10: Phase 2 Started. Waiting for damonfinal defeat...</color>");
            
            // Safety: Wait 2 seconds so Unity runs Start() on all new/enabled scripts and health initializes!
            yield return new WaitForSeconds(2f); 
            
            yield return new WaitUntil(() => {
                // If either component confirms defeat, trigger the ending
                if (slimeBoss != null && slimeBoss.IsDefeated()) return true;
                if (finaleBoss != null && finaleBoss.IsDefeated()) return true;
                if (eh != null && eh.currentHealth <= 0) return true;
                
                // Fallback: If no components are found, do not skip
                if (slimeBoss == null && finaleBoss == null && eh == null) return false;
                
                return false;
            });
            
            Debug.Log("<color=green>Scene 10: damonfinal Defeated! Final sequence starting.</color>");
            
            // --- STOP BOSS MOVEMENT AND ANIMATIONS IMMEDIATELY ---
            if (slimeBoss != null) slimeBoss.enabled = false;
            if (finaleBoss != null) finaleBoss.enabled = false;
            
            Animator bossAnim = activeFinalBoss.GetComponentInChildren<Animator>();
            if (bossAnim != null)
            {
                if (slimeBoss != null && slimeBoss.idleClip != null)
                    bossAnim.Play(slimeBoss.idleClip.name);
                else if (finaleBoss != null && finaleBoss.idle != null)
                    bossAnim.Play(finaleBoss.idle.name);
                else
                    bossAnim.Play("Idle");

                bossAnim.SetBool("isWalking", false);
                bossAnim.SetBool("isChasing", false);
            }

            Rigidbody2D bossRb = activeFinalBoss.GetComponent<Rigidbody2D>();
            if (bossRb != null) bossRb.linearVelocity = Vector2.zero;

            // --- START PULSING RED INDICATOR EFFECT ---
            SpriteRenderer bossSr = activeFinalBoss.GetComponentInChildren<SpriteRenderer>();
            if (bossSr != null)
            {
                bossPulseCoroutine = StartCoroutine(PulseBossRed(bossSr));
            }
        }
        else
        {
            Debug.LogError("Scene 10 FATAL: 'damonFinal' is NOT assigned in the Inspector! The game is skipping to the end because it has no boss to fight.");
        }

        // 4. THE DEFEAT & CONVERSATION
        StartCoroutine(FadeBGM(endingBGM));
        if (player != null) 
        {
            player.enabled = false;
            
            // Stop any ongoing movement/sliding immediately
            Rigidbody2D pRb = player.GetComponent<Rigidbody2D>();
            if (pRb != null)
            {
                pRb.linearVelocity = Vector2.zero;
                pRb.linearVelocity = Vector2.zero; // Dual support for older/newer Unity physics
            }
            
            // Force player animator to idle so they aren't stuck in a walking pose
            Animator pAnim = player.GetComponentInChildren<Animator>();
            if (pAnim != null) pAnim.Play("KnightIdle");
        }
        yield return PlayDialogue(damonDefeatedLines);

        // 5. CREDITS (Smooth transition right after Tala & Yves dialogue ends!)
        yield return StartCoroutine(ShowCredits());
    }

    IEnumerator FadeBGM(AudioClip nextClip)
    {
        if (bgmSource == null || bgmSource.clip == nextClip) yield break;

        // Fade out
        if (bgmSource.isPlaying)
        {
            while (bgmSource.volume > 0)
            {
                bgmSource.volume -= Time.deltaTime / musicFadeSpeed;
                yield return null;
            }
        }

        bgmSource.clip = nextClip;
        if (nextClip == null) yield break;

        bgmSource.Play();

        // Fade in
        while (bgmSource.volume < 1.0f)
        {
            bgmSource.volume += Time.deltaTime / musicFadeSpeed;
            yield return null;
        }
    }

    IEnumerator PlayDialogue(LBYH_Line[] lines)
    {
        if (lines == null) yield break;
        foreach (var line in lines)
        {
            // Custom logic for the eye reveal - explicitly check with Unity's overloaded != operator to avoid pseudo-null C# gotchas!
            try
            {
                if (line.text != null && line.text.Contains("MY EYE!"))
                {
                    if (puddleEyeReveal != null) puddleEyeReveal.SetActive(true);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Dialogue] Failed to activate puddleEyeReveal: {ex.Message}");
            }

            // Custom logic for Damon Disappearing & Screen Shaking
            try
            {
                if (line.text != null && line.text.Contains("Damon Disappears"))
                {
                    // Trigger screenshake safely!
                    StartCoroutine(ShakeScreen(3f, 0.5f));

                    // Stop the pulsing coroutine if running
                    if (bossPulseCoroutine != null)
                    {
                        StopCoroutine(bossPulseCoroutine);
                        bossPulseCoroutine = null;
                    }

                    GameObject activeFinalBoss = damonFinal != null ? damonFinal : finalBossObject;
                    if (activeFinalBoss != null)
                    {
                        // Restore original color if still visible, then deactivate
                        SpriteRenderer bossSr = activeFinalBoss.GetComponentInChildren<SpriteRenderer>();
                        if (bossSr != null) bossSr.color = Color.white;
                        activeFinalBoss.SetActive(false);
                    }
                    if (damonBoss != null) damonBoss.gameObject.SetActive(false);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Dialogue] Failed in Damon Disappears block: {ex.Message}");
            }

            // Stop any ongoing dialogue audio immediately on both sources
            if (voiceSource != null) voiceSource.Stop();
            if (dialogueUI != null && dialogueUI.audioSource != null) dialogueUI.audioSource.Stop();

            // Play voice if present
            if (line.voiceClip != null && voiceSource != null)
            {
                voiceSource.clip = line.voiceClip;
                voiceSource.Play();
            }
            
            dialogueUI.PresentLine(line);
            while (dialogueUI.IsTyping) yield return null;
            yield return WaitForInput();
        }
        dialogueUI.HideDialoguePanel();
    }

    IEnumerator TeleportToFinalArena()
    {
        // 1. Fade to Black
        if (blackScreen != null)
        {
            float t = 0;
            while (t < 1f) { t += Time.deltaTime * 2.5f; blackScreen.alpha = t; yield return null; }
        }
        else
        {
            Debug.LogWarning("Scene 10: Black Screen (CanvasGroup) is missing! Teleporting without fade.");
        }

        // 2. Disable Human Form
        if (damonBoss != null) damonBoss.gameObject.SetActive(false);

        // 3. Teleport Player
        if (player != null && bossArenaTeleportPoint != null)
        {
            Rigidbody2D pRb = player.GetComponent<Rigidbody2D>();
            if (pRb != null) pRb.simulated = false; // Freeze physics so we don't fall through the floor!
            
            player.transform.position = bossArenaTeleportPoint.position;
            
            if (pRb != null) pRb.simulated = true; // Unfreeze
            
            // Re-center camera
            CameraFollow cam = FindAnyObjectByType<CameraFollow>();
            if (cam != null) cam.target = player.transform;
        }

        yield return new WaitForSeconds(1.5f); // Pause in darkness

        // 4. Fade back in
        if (blackScreen != null)
        {
            float t = 1f;
            while (t > 0f) { t -= Time.deltaTime * 1.5f; blackScreen.alpha = t; yield return null; }
        }
    }

    IEnumerator DestructionCinematic()
    {
        // World Shaking, Damon Disappears
        if (damonBoss != null) damonBoss.gameObject.SetActive(false);
        GameObject activeFinalBoss = damonFinal != null ? damonFinal : finalBossObject;
        if (activeFinalBoss != null) activeFinalBoss.SetActive(false);
        
        yield return StartCoroutine(ShakeScreen(3f, 0.5f));
        
        // Fade to black
        float t = 0;
        while (t < 2f)
        {
            t += Time.deltaTime;
            blackScreen.alpha = t / 2f;
            yield return null;
        }
    }

    IEnumerator ClassroomSequence()
    {
        if (classroomSet != null) classroomSet.SetActive(true);
        yield return new WaitForSeconds(1f);
        
        float t = 1;
        while (t > 0)
        {
            t -= Time.deltaTime;
            blackScreen.alpha = t;
            yield return null;
        }

        yield return PlayDialogue(classroomLines);
    }

    IEnumerator ShowCredits()
    {
        if (!string.IsNullOrEmpty(creditsSceneName))
        {
            Debug.Log($"Scene 10: Transitioning to dedicated Credits Scene: {creditsSceneName}");
            SceneManager.LoadScene(creditsSceneName);
            yield break;
        }

        if (blackScreen != null) blackScreen.alpha = 1;
        if (creditsPanel != null) creditsPanel.SetActive(true);
        yield return new WaitForSeconds(10f); 
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    IEnumerator WaitForInput()
    {
        yield return null;
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));
    }

    IEnumerator ShakeScreen(float duration, float magnitude)
    {
        CameraFollow cf = FindAnyObjectByType<CameraFollow>();
        if (cf != null) cf.enabled = false; // Disable camera tracking so the shake offsets are visible!

        Camera targetCam = Camera.main;
        if (targetCam == null && cf != null) targetCam = cf.GetComponent<Camera>();
        if (targetCam == null) targetCam = FindAnyObjectByType<Camera>();
        
        if (targetCam != null)
        {
            Vector3 originalPos = targetCam.transform.position;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                targetCam.transform.position = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
                elapsed += Time.deltaTime;
                yield return null;
            }
            targetCam.transform.position = originalPos;
        }

        if (cf != null) cf.enabled = true; // Re-enable tracking!
    }

    IEnumerator PulseBossRed(SpriteRenderer sr)
    {
        if (sr == null) yield break;
        Color originalColor = sr.color;
        while (true)
        {
            // Ping-pong color between white/original and deep red
            float t = Mathf.PingPong(Time.time * 4f, 1f);
            sr.color = Color.Lerp(Color.white, Color.red, t);
            yield return null;
        }
    }
}
