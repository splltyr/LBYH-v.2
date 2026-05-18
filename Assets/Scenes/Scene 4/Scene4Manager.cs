using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Scene4Manager : MonoBehaviour
{
    [Header("Character References")]
    public RomeNPC romeScript; 
    public GameObject reyaObject; 
    public GameObject playerObject; 
    private KnightHero playerComponent;

    [Header("Boss Configuration")]
    public GameObject misTankBoss; 
    public Transform bossSpawnPoint;

    [Header("Movement Points")]
    public Transform romeStopPoint;
    public Transform romeFinalTalkPoint; // --- NEW: Rome walks here after boss dies ---
    public Transform reyaStopPoint;
    public Transform reyaExitPoint; 
    public float walkSpeed = 3f;

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName = "Scene 5";
    [SerializeField] private Image fadeOverlay;

    [Header("Dialogue System")]
    [SerializeField] private GameObject dialogueUIObject;
    private LBYH_Dialogue dialogueUI;

    [Header("Dialogue Content")]
    [SerializeField] private LBYH_Line[] introLines = new LBYH_Line[] {
        new LBYH_Line { name = "Yves", text = "Huh, the keycard worked to get into the building. Anyway, are the elevators worki-" },
        new LBYH_Line { name = "Rome", text = "Hey Reya, did you get the food rations back— ?! WHO ARE YOU?! Wait… how did you get that key card?!" },
        new LBYH_Line { name = "Yves", text = "Oh? This? Well actually—" },
        new LBYH_Line { name = "Rome", text = "WHO THE HELL ARE YOU?!" },
        new LBYH_Line { name = "Yves", text = "Hold on! Let me explain!" },
        new LBYH_Line { name = "Rome", text = "I DON'T NEED ANY EXPLANATION FROM YOU MONSTERS! YOU PROBABLY WORK FOR [REDACTED]!" },
        new LBYH_Line { name = "Yves", text = "What?! Work for who? No! I’m just a student here!" },
        new LBYH_Line { name = "Rome", text = "LIAR! There’s no students here! Who sent you?!" },
        new LBYH_Line { name = "Yves", text = "At least listen to me! Cook Re—" }
    };

    [SerializeField] private LBYH_Line[] reyaConversation = new LBYH_Line[] {
        new LBYH_Line { name = "Tala", text = "OH! Cook Reya is here! Hang in there, Yves!" },
        new LBYH_Line { name = "Cook Reya", text = "WHAT HAPPENED HERE? Oh.. poor kid. You.." },
        new LBYH_Line { name = "Rome", text = "Reya, you owe me one. I swear." },
        new LBYH_Line { name = "Cook Reya", text = "I sent him here.. And this is what you’ll do to him?!" },
        new LBYH_Line { name = "Rome", text = "I didn’t know.." },
        new LBYH_Line { name = "Tala", text = "Oh look, it’s Cook Reya!" },
        new LBYH_Line { name = "Cook Reya", text = "*lets out a big sigh* Unbelievable." },
        new LBYH_Line { name = "Cook Reya", text = "Rome, stop screaming at a kid." }
    };

    [SerializeField] private LBYH_Line[] romeApology = new LBYH_Line[] {
        new LBYH_Line { name = "Rome", text = "Y-Yes Ma’am.." },
        new LBYH_Line { name = "Yves", text = "Rome..?" },
        new LBYH_Line { name = "Rome", text = "Sorry about earlier, I was.. A bit too cautious. As an apology, I give you this." },
        new LBYH_Line { name = "Tala", text = "Yeah right.. BLABLABLA" },
        new LBYH_Line { name = "Narrative", text = "[Player drinks something, now has +5 ATK]" },
        new LBYH_Line { name = "Rome", text = "I’m Rome by the way. One of the faculty members here in STI Caloocan. I’m one of the IT specialists." }
    };

    [SerializeField] private LBYH_Line[] bossArrivalLines = new LBYH_Line[] {
        new LBYH_Line { name = "Narrative", text = "[A sudden burst from the faculty entrance shakes the screen, and a shriek echoes the room]" },
        new LBYH_Line { name = "???", text = "MESSY! MESSY! PAPER EVERYWHERE!" },
        new LBYH_Line { name = "Rome", text = "Damn… He came back fast." },
        new LBYH_Line { name = "Yves", text = "What is that???" },
        new LBYH_Line { name = "Tala", text = "A corrupted faculty member. He was once a part of the MIS, something went wrong and he fused with the servers." },
        new LBYH_Line { name = "Rome", text = "He was once a friend of mine… " },
        new LBYH_Line { name = "Yves", text = "You mean, that was once a person?" },
        new LBYH_Line { name = "Rome", text = "Yep… Before that damn guy turned everything upside down." },
        new LBYH_Line { name = "Yves", text = "By that guy… Do you mean…?" },
        new LBYH_Line { name = "Rome", text = "[REDACTED]. Who else? " },
        new LBYH_Line { name = "Yves", text = "I didn’t hear y-" },
        new LBYH_Line { name = "Tala", text = "Yves! Be cautious." },
        new LBYH_Line { name = "Rome", text = "Damn, he’s angry. He must’ve found out I took some keycards again." },
        new LBYH_Line { name = "Yves", text = "What exactly do you do, Sir Rome? How did you survive this long here?" },
        new LBYH_Line { name = "Rome", text = "I try to help people live in this hell of a place. As you can see, with my skills, I can tweak stuff here and there. But that requires me to steal some data here in the faculty." },
        new LBYH_Line { name = "Rome", text = "Let me see your skills, Yves. Let’s see whether that investment of mine was a good gamble." },
        new LBYH_Line { name = "Tala", text = "You can do this, Yves! I’ll guide you." }
    };

    [SerializeField] private LBYH_Line[] bossDefeatedLines = new LBYH_Line[] {
        new LBYH_Line { name = "Rome", text = "Not bad, kid. I see potential." },
        new LBYH_Line { name = "Yves", text = "Uh.. Thanks. Where are you headed now?" },
        new LBYH_Line { name = "Rome", text = "I need to assist Reya with the food rations. The corruption is starting to spread throughout the STEM Laboratory in the 2nd and 4th floors. We need to evacuate civilians still in this building." },
        new LBYH_Line { name = "Tala", text = "Oooo! That’s great, we’ll be going there anyway!" },
        new LBYH_Line { name = "Yves", text = "I can help. I need to go up too." },
        new LBYH_Line { name = "Rome", text = "Why?" },
        new LBYH_Line { name = "Tala", text = "You can’t tell him, Yves.. [GLITCHY VOICE]" },
        new LBYH_Line { name = "Yves", text = "I… don’t know actually." },
        new LBYH_Line { name = "Rome", text = "You’re a strange kid, aren’t you? Welp, here’s the keycard to pass through the 2nd floor." },
        new LBYH_Line { name = "Rome", text = "Be careful, sneaks roam around the place. Things are a lot worse up there. It’s like a different place." },
        new LBYH_Line { name = "Tala", text = "Oop- He’s right." },
        new LBYH_Line { name = "Yves", text = "Okay, thanks." },
        new LBYH_Line { name = "Rome", text = "If ever you need some leveling up, just come here. I’ll invest more if you need. And you might come across Dave. I think he’ll be able to help you too." },
        new LBYH_Line { name = "Yves", text = "Will do, see you Sir Rome." },
        new LBYH_Line { name = "Rome", text = "Take care kid." },
        new LBYH_Line { name = "Tala", text = "He’s a good man after all.." },
        new LBYH_Line { name = "Yves", text = "Why did you say that I can't tell Sir Rome? Tell him what, exactly?" },
        new LBYH_Line { name = "Tala", text = "…" },
        new LBYH_Line { name = "Narrative", text = "[Tala stays quiet, as she knows that Yves couldn’t remember anything because of the time loop.]" }
    };

    private bool bossHasSpawned = false;

    IEnumerator Start()
    {
        if (playerObject != null && !playerObject.Equals(null)) 
            playerComponent = playerObject.GetComponent<KnightHero>();

        if (dialogueUIObject != null)
        {
            dialogueUI = dialogueUIObject.GetComponent<LBYH_Dialogue>();
            if (dialogueUI == null) dialogueUI = dialogueUIObject.GetComponentInChildren<LBYH_Dialogue>();
        }
        if (dialogueUI == null) dialogueUI = FindAnyObjectByType<LBYH_Dialogue>();

        if (dialogueUI == null) yield break;

        if (reyaObject != null) reyaObject.SetActive(false);
        if (misTankBoss != null) misTankBoss.SetActive(false);
        
        SetPlayerActive(false);

        if (romeScript != null)
        {
            romeScript.enabled = false;
            foreach (var comp in romeScript.GetComponents<MonoBehaviour>())
            {
                if (comp.GetType().Name.Contains("Dialogue") || comp.GetType().Name.Contains("NPC"))
                {
                    if (comp != this) comp.enabled = false;
                }
            }
        }

        StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        // 1. INTRO
        if (romeScript != null)
        {
            romeScript.gameObject.SetActive(true);
            StartCoroutine(MoveCharacter(romeScript.transform, romeStopPoint.position, "RomeWalk"));
        }
        yield return new WaitForSeconds(0.1f);
        yield return PlayDialogue(introLines);

        // 2. REYA ARRIVAL
        if (reyaObject != null)
        {
            reyaObject.SetActive(true);
            yield return StartCoroutine(MoveCharacter(reyaObject.transform, reyaStopPoint.position, "ReyaWalk"));
        }
        yield return PlayDialogue(reyaConversation);

        // ACTION: REYA LEAVES
        if (reyaObject != null)
        {
            StartCoroutine(MoveCharacter(reyaObject.transform, reyaExitPoint.position, "ReyaWalk"));
        }

        // 3. ROME APOLOGY
        yield return PlayDialogue(romeApology);
        if (reyaObject != null) reyaObject.SetActive(false);

        // Apply ATK Buff
        if (playerComponent != null) playerComponent.attackDamage += 5f;

        // 4. BOSS ARRIVAL
        yield return PlayDialogue(bossArrivalLines);
        
        if (misTankBoss != null)
        {
            misTankBoss.transform.position = bossSpawnPoint.position;
            misTankBoss.SetActive(true);
            bossHasSpawned = true;
        }

        yield return StartCoroutine(ShakeScreen(2.5f, 0.5f));
        SetPlayerActive(true);

        // 5. WAIT FOR BOSS DEFEAT
        yield return new WaitUntil(() => IsBossDefeated());
        yield return new WaitForSeconds(1f); // Brief pause after death

        // --- NEW ACTION: Rome walks to Yves for final talk ---
        SetPlayerActive(false);
        if (romeScript != null && romeFinalTalkPoint != null)
        {
            yield return StartCoroutine(MoveCharacter(romeScript.transform, romeFinalTalkPoint.position, "RomeWalk"));
        }

        // 6. POST-BOSS DIALOGUE
        yield return PlayDialogue(bossDefeatedLines);

        // 7. FADE OUT & NEXT SCENE
        if (fadeOverlay != null) yield return FadeScreen(1f);
        SceneManager.LoadScene(nextSceneName);
    }

    void SetPlayerActive(bool state)
    {
        if (playerComponent != null)
        {
            playerComponent.enabled = state;
            Rigidbody2D rb = playerComponent.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            Animator anim = playerComponent.GetComponentInChildren<Animator>();
            if (!state && anim != null) anim.Play("KnightIdle");
        }
    }

    IEnumerator PlayDialogue(LBYH_Line[] lines)
    {
        if (lines == null || lines.Length == 0) yield break;
        for (int i = 0; i < lines.Length; i++)
        {
            // --- NEW: Add the name directly to the text string ---
            LBYH_Line lineToDisplay = lines[i];
            if (lineToDisplay.name != "Narrative" && !string.IsNullOrEmpty(lineToDisplay.name))
            {
                lineToDisplay.text = lineToDisplay.name + ": " + lineToDisplay.text;
            }

            dialogueUI.PresentLine(lineToDisplay);
            while (dialogueUI.IsTyping) yield return null;
            yield return WaitForInput();
        }
        dialogueUI.HideDialoguePanel();
    }

    IEnumerator WaitForInput()
    {
        yield return null;
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));
    }

    IEnumerator MoveCharacter(Transform character, Vector3 target, string animParam)
    {
        if (character == null) yield break;
        Animator anim = character.GetComponent<Animator>();
        if (anim != null && HasParameter(anim, animParam)) anim.SetBool(animParam, true);

        // --- FIXED: Inverted flip (Rome seems to face Left by default) ---
        float direction = target.x > character.position.x ? -1 : 1;
        character.localScale = new Vector3(Mathf.Abs(character.localScale.x) * direction, character.localScale.y, character.localScale.z);

        float startTime = Time.time;
        while (Vector3.Distance(character.position, target) > 0.1f)
        {
            if (Time.time - startTime > 10f) break;
            character.position = Vector3.MoveTowards(character.position, target, walkSpeed * Time.deltaTime);
            yield return null;
        }
        if (anim != null && HasParameter(anim, animParam)) anim.SetBool(animParam, false);
    }

    IEnumerator ShakeScreen(float duration, float magnitude)
    {
        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            Camera.main.transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.localPosition = originalPos;
    }

    IEnumerator FadeScreen(float targetAlpha)
    {
        if (fadeOverlay == null) yield break;
        float startAlpha = fadeOverlay.color.a;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            Color c = fadeOverlay.color;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            fadeOverlay.color = c;
            yield return null;
        }
    }

    bool IsBossDefeated()
    {
        if (!bossHasSpawned) return false;
        if (misTankBoss == null) return true;
        EnemyHealth eh = misTankBoss.GetComponent<EnemyHealth>();
        if (eh != null) return eh.currentHealth <= 0;
        return !misTankBoss.activeInHierarchy;
    }

    public void ShowReya() { }
    public void StartBossEvent() { }

    private bool HasParameter(Animator animator, string paramName)
    {
        foreach (var param in animator.parameters) if (param.name == paramName) return true;
        return false;
    }
}