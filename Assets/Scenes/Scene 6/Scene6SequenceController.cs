using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene6SequenceController : MonoBehaviour
{
    [Header("UI & Dialogue")]
    [SerializeField] private LBYH_Dialogue dialogueUI;
    [SerializeField] private CanvasGroup screenFader;
    [SerializeField] private string nextSceneName = "Scene 7";

    [Header("Combat References")]
    [SerializeField] private GameObject wave1Enemies; // Sneaks and Mechanics
    [SerializeField] private GameObject janitorBoss;
    [SerializeField] private GameObject sirDaveNPC;
    [SerializeField] private GameObject realJanitorHuman; // Hidden until boss death

    [Header("Positions")]
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private Transform exitPoint;

    [Header("BGM/SFX (Optional)")]
    [SerializeField] private AudioSource ambientAudio;
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D globalLight; // Optional for flickering

    [Header("BGM Setup")]
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] [Range(0f, 1f)] private float bgmVolume = 0.3f;
    private AudioSource bgmSource;

    // --- DIALOGUE DATA (Serialized so you can assign AudioClips in Inspector) ---
    [Header("Dialogue Blocks")]
    [SerializeField] private LBYH_Line[] introLines;
    [SerializeField] private LBYH_Line[] rescueCallLines;
    [SerializeField] private LBYH_Line[] meetDaveLines;
    [SerializeField] private LBYH_Line[] bossIntroLines;
    [SerializeField] private LBYH_Line[] postBossLines;

    [ContextMenu("Initialize Dialogue Text")]
    public void InitializeDialogue()
    {
        introLines = new LBYH_Line[] {
            new LBYH_Line { name = "Yves", text = "Ugh, How’s my luck worse than this?" },
            new LBYH_Line { name = "Tala", text = "Good thing that I don’t have legs." },
            new LBYH_Line { name = "Tala", text = "Yves 0, Tala 1. Muehehehe" },
            new LBYH_Line { name = "Yves", text = "I didn’t know that your name is Tala." },
            new LBYH_Line { name = "Tala", text = "Well, I never really got the chance to introduce myself." },
            new LBYH_Line { name = "Yves", text = "Hmm.. Who exactly are you?" },
            new LBYH_Line { name = "Tala", text = "Glad you asked, even if it’s late. Haloo! I am Tala, I am a spirit that roams around here in this world. I have longed for my purpose, and then.. You came." },
            new LBYH_Line { name = "Yves", text = "Purpose, huh? Your purpose is to be a glowy bubble that chatters a lot?" },
            new LBYH_Line { name = "Tala", text = "HEY! But really, my purpose is to guide y-" }
        };

        rescueCallLines = new LBYH_Line[] {
            new LBYH_Line { name = "Male Voice", text = "I swear on every deity. If you don- AGH!" },
            new LBYH_Line { name = "Tala", text = "*sighs* Aaand my words get cut off again.. That way, Yves!" },
            new LBYH_Line { name = "Yves", text = "Let’s go!" }
        };

        meetDaveLines = new LBYH_Line[] {
            new LBYH_Line { name = "Yves", text = "Are you hurt?" },
            new LBYH_Line { name = "???", text = "What deity are you to save me, young man?" },
            new LBYH_Line { name = "Tala", text = "Wow, Yves? A deity? He’s funny." },
            new LBYH_Line { name = "Yves", text = "Uh.. I am no deity, Sir..?" },
            new LBYH_Line { name = "Sir Dave", text = "Ah, apologies.. I swore that enemy to a deity. Oh, right. Allow me to introduce myself. My name is Dave, I was once a Professor here on STI." },
            new LBYH_Line { name = "Yves", text = "Sir Dave? Oh, Sir Rome told me that you might be able to help me.. Is that right?" },
            new LBYH_Line { name = "Tala", text = "It is right.." },
            new LBYH_Line { name = "Sir Dave", text = "Correct. I can upgrade your weapons and make you even stronger." },
            new LBYH_Line { name = "Tala", text = "That would be awesome!" },
            new LBYH_Line { name = "Sir Dave", text = "May I ask if Reya and Rome are doing fine?" },
            new LBYH_Line { name = "Yves", text = "Yes, Sir Dave. I made sure that they’re safe before coming here." },
            new LBYH_Line { name = "Sir Dave", text = "So that would mean that you have defeated the Corrupted’s, yes?" },
            new LBYH_Line { name = "Yves", text = "Uh.. Yes sir." },
            new LBYH_Line { name = "Tala", text = "It’s almost like an interview.." },
            new LBYH_Line { name = "Sir Dave", text = "Let me see what I can do for you." }
        };

        bossIntroLines = new LBYH_Line[] {
            // Narrative lines replaced by Cinematic Tremble Effect!
            new LBYH_Line { name = "Yves", text = "That doesn’t smell good." },
            new LBYH_Line { name = "Tala", text = "That doesn’t look good." },
            new LBYH_Line { name = "Sir Dave", text = "Kid… get ready. That’s not just movement… that’s the boss!" },
            new LBYH_Line { name = "Corrupted Janitor", text = "CLLLEEEAAANNN… EVERTHING MUST BE CLEAN… FOR MASTER [REDACTED]!" },
            new LBYH_Line { name = "Yves", text = "Yeah… definitely corrupted… A Weird looking ONE!?" },
            new LBYH_Line { name = "Tala", text = "Be careful! He charges fast—wait for the right moment!" }
        };

        postBossLines = new LBYH_Line[] {
            new LBYH_Line { name = "Janitor", text = "W-Where’s my d-daughter.. Aghh! it hurts.." },
            new LBYH_Line { name = "Yves", text = "Sir Dave, please help!" },
            new LBYH_Line { name = "Janitor", text = "Thank you. I..I thought… I was done for…" },
            new LBYH_Line { name = "Sir Dave", text = "You’re safe now." },
            new LBYH_Line { name = "Janitor", text = "Take this… it opens the next floor… stop… that thing…" },
            new LBYH_Line { name = "Yves", text = "But what about the perso-" },
            new LBYH_Line { name = "Sir Dave", text = "I’ll take him somewhere safe. You keep moving, kid." },
            new LBYH_Line { name = "Yves", text = "O-Okay.." },
            new LBYH_Line { name = "Yves", text = "Hey Tala, how come that the other Corrupt's that we’ve killed doesn’t have a living person inside it.. Unlike the Janitor?" },
            new LBYH_Line { name = "Tala", text = "I don’t think I can explain it to you in full detail.. But let’s just say that his body was corrupted, but his soul is pure." },
            new LBYH_Line { name = "Yves", text = "I still don’t understand.." },
            new LBYH_Line { name = "Tala", text = "Let’s focus on what’s ahead, we should be cautious." }
        };
        Debug.Log("Scene 6: Dialogue initialized. You can now assign AudioClips in the Inspector.");
    }

    private JanitorBossController janitorCtrl;
    private bool hasCrashedOnce = false;

    void Start()
    {
        // Setup and play BGM automatically
        if (bgmClip != null)
        {
            GameObject bgmGO = new GameObject("SceneBGM");
            bgmGO.transform.SetParent(transform);
            bgmSource = bgmGO.AddComponent<AudioSource>();
            bgmSource.clip = bgmClip;
            bgmSource.volume = bgmVolume;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        if (dialogueUI == null) dialogueUI = FindAnyObjectByType<LBYH_Dialogue>();
        
        // Initial setup: hide enemies/boss
        if (wave1Enemies != null) wave1Enemies.SetActive(false);
        if (janitorBoss != null) janitorBoss.SetActive(false);
        if (realJanitorHuman != null) realJanitorHuman.SetActive(false);
        if (screenFader != null) screenFader.alpha = 0;

        StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        // 1. INTRO DIALOGUE
        yield return PlayDialogue(introLines);

        // 2. RESCUE CALL & ENEMY SPAWN
        yield return PlayDialogue(rescueCallLines);
        if (wave1Enemies != null) wave1Enemies.SetActive(true);

        // 3. WAIT FOR WAVE 1 DEFEAT
        Debug.Log("Scene6: Waiting for enemies to be defeated...");
        yield return new WaitUntil(() => IsDead(wave1Enemies));
        yield return new WaitForSeconds(1.5f);

        // 4. MEET SIR DAVE
        yield return PlayDialogue(meetDaveLines);

        // 5. UPGRADE WEAPONS MOMENT
        Debug.Log("Scene6: Upgrading weapons...");
        yield return Fade(1f, 0.5f);
        yield return new WaitForSeconds(1.5f);
        yield return Fade(0f, 0.5f);

        // 6. BOSS SPAWN SEQUENCE
        yield return StartCoroutine(PlayTrembleEffect());
        yield return PlayDialogue(bossIntroLines);
        if (janitorBoss != null)
        {
            if (bossSpawnPoint != null) janitorBoss.transform.position = bossSpawnPoint.position;
            janitorBoss.SetActive(true);
            
            // Disable auto-destroy so the boss GameObject stays active for post-combat animations/dialogue
            EnemyHealth eh = janitorBoss.GetComponent<EnemyHealth>();
            if (eh != null) eh.autoDestroy = false;

            janitorCtrl = janitorBoss.GetComponent<JanitorBossController>();
            if (janitorCtrl != null)
            {
                janitorCtrl.OnCrashed += HandleBossCrash;
            }
        }

        // 7. BOSS FIGHT START DIALOGUE
        yield return PlayDialogue(new LBYH_Line[] { 
            new LBYH_Line { name = "Corrupted Janitor", text = "FILTH DETECTED!!!" },
            new LBYH_Line { name = "Yves", text = "Eugh.. Disgusting." }
        });

        // Start background combat banter monitor
        StartCoroutine(BossFightBanter());

        // 8. WAIT FOR BOSS DEFEAT
        Vector3 bossDeathPosition = janitorBoss != null ? janitorBoss.transform.position : Vector3.zero;
        while (!IsDead(janitorBoss))
        {
            if (janitorBoss != null) bossDeathPosition = janitorBoss.transform.position;
            yield return null;
        }
        Debug.Log("Scene6: Boss defeated!");

        // 9. POST-BOSS SEQUENCE
        if (janitorBoss != null)
        {
            // Disable combat AI and physics so he stands peacefully for the dialogue
            JanitorBossController ctrl = janitorBoss.GetComponent<JanitorBossController>();
            if (ctrl != null) ctrl.enabled = false;
            
            Rigidbody2D rb = janitorBoss.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
            
            foreach (var col in janitorBoss.GetComponentsInChildren<Collider2D>())
            {
                col.enabled = false;
            }
            
            // Play the "Human" animation state directly on the boss's Animator!
            Animator anim = janitorBoss.GetComponentInChildren<Animator>();
            if (anim != null) anim.Play("Human");
        }
        
        if (realJanitorHuman != null) realJanitorHuman.SetActive(false); // Hide the extra placeholder
        
        yield return PlayDialogue(postBossLines);

        // 10. END SCENE
        yield return Fade(1f, 2f);
        Debug.Log("Scene 6 Complete. Transitioning to " + nextSceneName);
        SceneManager.LoadScene(nextSceneName); // Automatically load the next scene!
    }

    private void HandleBossCrash()
    {
        if (!hasCrashedOnce)
        {
            hasCrashedOnce = true;
            StartCoroutine(PlayQuickBanter(new LBYH_Line { name = "Tala", text = "That’s your cue, Yves!" }));
        }
    }

    IEnumerator BossFightBanter()
    {
        if (janitorBoss == null) yield break;
        EnemyHealth eh = janitorBoss.GetComponent<EnemyHealth>();
        bool playedMovementBanter = false;

        while (eh != null && eh.currentHealth > 0)
        {
            // Trigger banter at health threshold or after crash
            if (!playedMovementBanter && eh.currentHealth < 300)
            {
                playedMovementBanter = true;
                yield return PlayQuickBanter(new LBYH_Line[] {
                    new LBYH_Line { name = "Yves", text = "This water’s making it harder to move!" },
                    new LBYH_Line { name = "Tala", text = "Use the terrain to make him crash again!" }
                });
            }
            yield return new WaitForSeconds(1f);
        }
    }

    // --- HELPER FUNCTIONS ---

    IEnumerator PlayDialogue(LBYH_Line[] lines)
    {
        if (dialogueUI == null) yield break;
        foreach (var line in lines)
        {
            // Skip narrative lines so the player only sees spoken dialogue!
            if (line.name == "Narrative") continue;
            
            dialogueUI.PresentLine(line);
            yield return new WaitForSeconds(0.1f);
            while (dialogueUI.IsTyping) yield return null;
            yield return WaitForInput();
        }
        dialogueUI.HideDialoguePanel();
    }

    IEnumerator PlayQuickBanter(LBYH_Line line) { yield return PlayQuickBanter(new LBYH_Line[] { line }); }
    IEnumerator PlayQuickBanter(LBYH_Line[] lines)
    {
        if (dialogueUI == null) yield break;
        foreach (var line in lines)
        {
            dialogueUI.PresentLine(line);
            while (dialogueUI.IsTyping) yield return null;
            yield return new WaitForSeconds(1.5f); // Auto-advance during combat
        }
        dialogueUI.HideDialoguePanel();
    }

    IEnumerator WaitForInput()
    {
        yield return null;
        yield return new WaitUntil(() =>
            Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E));
    }

    IEnumerator Fade(float target, float duration)
    {
        if (screenFader == null) yield break;
        float start = screenFader.alpha;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            screenFader.alpha = Mathf.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        screenFader.alpha = target;
    }

    bool IsDead(GameObject obj)
    {
        if (obj == null) return true;
        EnemyHealth[] ehs = obj.GetComponentsInChildren<EnemyHealth>(true);
        BossHealth[] bhs = obj.GetComponentsInChildren<BossHealth>(true);
        
        if (ehs.Length == 0 && bhs.Length == 0)
        {
            Debug.LogError($"<color=red>SCENE SKIP BUG DETECTED: {obj.name} does NOT have an EnemyHealth script attached! It instantly skipped the fight!</color>");
            return false; // Prevent it from returning true and instantly skipping the boss!
        }

        foreach (var h in ehs) if (h != null && h.currentHealth > 0) return false;
        foreach (var h in bhs) if (h != null && h.health > 0) return false;
        return true;
    }

    IEnumerator PlayTrembleEffect()
    {
        float duration = 2.0f;
        float elapsed = 0f;
        
        Vector3 originalCamPos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        float originalLightInt = globalLight != null ? globalLight.intensity : 1f;

        while (elapsed < duration)
        {
            if (Camera.main != null)
            {
                float offsetX = Random.Range(-0.1f, 0.1f);
                float offsetY = Random.Range(-0.1f, 0.1f);
                Camera.main.transform.position = originalCamPos + new Vector3(offsetX, offsetY, 0);
            }
            
            if (globalLight != null)
            {
                globalLight.intensity = originalLightInt * Random.Range(0.2f, 1.2f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (Camera.main != null) Camera.main.transform.position = originalCamPos;
        if (globalLight != null) globalLight.intensity = originalLightInt;
        
        yield return new WaitForSeconds(0.5f);
    }
}
