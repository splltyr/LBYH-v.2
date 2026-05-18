using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Scene6Master : MonoBehaviour
{
    [Header("UI References")]
    public LBYH_Dialogue dialogueUI;
    public CanvasGroup blackScreen;

    [Header("Entities")]
    public Transform player;
    public GameObject sirDave;
    public GameObject janitorBoss;
    public GameObject humanJanitor;
    
    [Header("Enemy Groups")]
    public GameObject sneakContainer;
    public GameObject skeletonContainer;
    public GameObject sewerEnemyContainer;
    public GameObject keycardPrefab;

    [Header("Movement Marks")]
    public Transform daveMark;
    public Transform janitorMark;
    public Transform rescueMark;

    [Header("BGM & Effects")]
    public AudioSource bgmSource;
    public AudioClip bossBGM;
    public UnityEngine.Rendering.Universal.Light2D globalLight; // Optional for flickering

    // DIALOGUE ARRAYS (Using LBYH_Line struct)
    public LBYH_Line[] introDialogue = {
        new LBYH_Line { name = "Yves", text = "Ugh, How’s my luck worse than this?" },
        new LBYH_Line { name = "???", text = "Good thing that I don’t have legs." },
        new LBYH_Line { name = "???", text = "Yves 0, Tala 1. Muehehehe" },
        new LBYH_Line { name = "Yves", text = "I didn’t know that your name is Tala." },
        new LBYH_Line { name = "Tala", text = "Well, I never really got the chance to introduce myself." },
        new LBYH_Line { name = "Yves", text = "Hmm.. Who exactly are you?" },
        new LBYH_Line { name = "Tala", text = "Glad you asked, even if it’s late. Haloo! I am Tala, I am a spirit that roams around here in this world. I have longed for my purpose, and then.. You came." },
        new LBYH_Line { name = "Yves", text = "Purpose, huh? Your purpose is to be a glowy bubble that chatters a lot?" },
        new LBYH_Line { name = "Tala", text = "HEY! But really, my purpose is to guide y-" },
        new LBYH_Line { name = "???", text = "I swear on every deity. If you don- AGH!" },
        new LBYH_Line { name = "Tala", text = "*sighs* Aaand my words get cut off again.. That way, Yves!" },
        new LBYH_Line { name = "Yves", text = "Let’s go!" }
    };

    public LBYH_Line[] daveDialogue = {
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
        new LBYH_Line { name = "Sir Dave", text = "Let me see what I can do for you." },
        new LBYH_Line { name = "Sir Dave", text = "So what do you need?" }
    };

    public LBYH_Line[] bossIntro = {
        new LBYH_Line { name = "Yves", text = "That doesn’t smell good." },
        new LBYH_Line { name = "Tala", text = "That doesn’t look good." },
        new LBYH_Line { name = "Sir Dave", text = "Kid… get ready. That’s not just movement… that’s the boss!" },
        new LBYH_Line { name = "Corrupted Janitor", text = "CLLLEEEAAANNN… EVERTHING MUST BE CLEAN… FOR MASTER [REDACTED]!" },
        new LBYH_Line { name = "Yves", text = "Yeah… definitely corrupted… A Weird looking ONE!?" },
        new LBYH_Line { name = "Tala", text = "Be careful! He charges fast—wait for the right moment!" },
        new LBYH_Line { name = "Corrupted Janitor", text = "FILTH DETECTED!!!" }
    };

    public LBYH_Line[] bossDefeat = {
        new LBYH_Line { name = "Corrupted Janitor", text = "…clean…[REDACTED]..." },
        new LBYH_Line { name = "Janitor", text = "W-Where’s my d-daughter.. Aghh! it hurts.." },
        new LBYH_Line { name = "Yves", text = "Sir Dave, please help!" },
        new LBYH_Line { name = "Janitor", text = "Thank you. I..I thought… I was done for…" },
        new LBYH_Line { name = "Sir Dave", text = "You’re safe now." },
        new LBYH_Line { name = "Janitor", text = "Take this… it opens the next floor… stop… that thing…" },
        new LBYH_Line { name = "Yves", text = "But what about the perso-" },
        new LBYH_Line { name = "Sir Dave", text = "I’ll take him somewhere safe. You keep moving, kid." },
        new LBYH_Line { name = "Yves", text = "O-Okay.." }
    };

    public LBYH_Line[] endingDialogue = {
        new LBYH_Line { name = "Yves", text = "Hey Tala, how come that the other Corrupt's that we’ve killed doesn’t have a living person inside it.. Unlike the Janitor?" },
        new LBYH_Line { name = "Tala", text = "I don’t think I can explain it to you in full detail.. But let’s just say that his body was corrupted, but his soul is pure." },
        new LBYH_Line { name = "Yves", text = "I still don’t understand.." },
        new LBYH_Line { name = "Tala", text = "Let’s focus on what’s ahead, we should be cautious." }
    };

    void Start()
    {
        if (dialogueUI == null) dialogueUI = FindAnyObjectByType<LBYH_Dialogue>(FindObjectsInactive.Include);
        if (janitorBoss != null) janitorBoss.SetActive(false);
        if (humanJanitor != null) humanJanitor.SetActive(false);
        if (sirDave != null) sirDave.SetActive(false);
        if (blackScreen != null) blackScreen.alpha = 0;
        
        StartCoroutine(RunScene6Sequence());
    }

    IEnumerator RunScene6Sequence()
    {
        yield return new WaitForSeconds(1.5f);
        
        yield return StartCoroutine(PlayDialogueSequence(introDialogue));

        // Activate enemies if they aren't already active
        if (sneakContainer != null) sneakContainer.SetActive(true);
        if (skeletonContainer != null) skeletonContainer.SetActive(true);
        if (sewerEnemyContainer != null) sewerEnemyContainer.SetActive(true);

        // Wait for all minions to be defeated
        while (true)
        {
            int total = 0;
            if (sneakContainer != null) total += GetActiveChildrenCount(sneakContainer.transform);
            if (skeletonContainer != null) total += GetActiveChildrenCount(skeletonContainer.transform);
            if (sewerEnemyContainer != null) total += GetActiveChildrenCount(sewerEnemyContainer.transform);
            
            if (total == 0) break;
            yield return new WaitForSeconds(1f);
        }

        if (sirDave != null) 
        {
            sirDave.SetActive(true);
            if (daveMark != null) yield return StartCoroutine(MoveEntityHorizontal(sirDave.transform, daveMark.position.x, 3f));
        }

        yield return StartCoroutine(PlayDialogueSequence(daveDialogue));
        
        if (bgmSource != null && bossBGM != null) { bgmSource.clip = bossBGM; bgmSource.Play(); }
        
        if (janitorBoss != null) 
        {
            janitorBoss.SetActive(true);
            if (janitorMark != null) janitorBoss.transform.position = janitorMark.position;
        }

        // Cinematic Tremble Effect!
        yield return StartCoroutine(PlayTrembleEffect());

        yield return StartCoroutine(PlayDialogueSequence(bossIntro));
        
        // Let physics and Start() methods initialize for 1 second before we start checking if he's dead!
        yield return new WaitForSeconds(1f);

        // WAIT FOR BOSS DEATH
        yield return new WaitUntil(() => IsBossDefeated());

        if (janitorBoss != null) janitorBoss.SetActive(false);
        if (humanJanitor != null) 
        {
            // Snap human janitor to the ground
            Vector3 spawnPos = janitorMark != null ? janitorMark.position : Vector3.zero;
            spawnPos.z = 0;
            
            int layerMask = LayerMask.GetMask("Ground", "Default", "Terrain");
            if (layerMask == 0) layerMask = ~0;
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(spawnPos.x, spawnPos.y + 2f), Vector2.down, 20f, layerMask);
            if (hit.collider != null)
            {
                spawnPos.y = hit.point.y;
            }
            
            humanJanitor.transform.position = spawnPos;
            humanJanitor.SetActive(true);
        }
        
        yield return StartCoroutine(PlayDialogueSequence(bossDefeat));
        
        if (sirDave != null && humanJanitor != null && rescueMark != null)
        {
            yield return StartCoroutine(MoveEntityHorizontal(sirDave.transform, humanJanitor.transform.position.x - 1.5f, 2f));
            if (keycardPrefab != null) Instantiate(keycardPrefab, humanJanitor.transform.position + Vector3.up, Quaternion.identity);
            
            StartCoroutine(MoveEntityHorizontal(humanJanitor.transform, rescueMark.position.x, 2f));
            yield return StartCoroutine(MoveEntityHorizontal(sirDave.transform, rescueMark.position.x, 2f));
            
            sirDave.SetActive(false);
            humanJanitor.SetActive(false);
        }

        yield return StartCoroutine(PlayDialogueSequence(endingDialogue));

        float elapsed = 0;
        while (elapsed < 2f) 
        { 
            if (blackScreen != null) blackScreen.alpha = elapsed / 2f; 
            elapsed += Time.deltaTime; 
            yield return null; 
        }
        
        Debug.Log("SCENE 6 COMPLETE!");
        // The level is complete. The player can walk to the level exit trigger or arrow!
    }

    private int GetActiveChildrenCount(Transform container)
    {
        int count = 0;
        foreach (Transform child in container)
        {
            if (child.gameObject.activeInHierarchy) count++;
        }
        return count;
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

    IEnumerator MoveEntityHorizontal(Transform entity, float targetX, float speed)
    {
        Animator anim = entity.GetComponent<Animator>();
        if (anim != null) anim.SetBool("isRunning", true);

        // Flip entity if needed
        float directionX = targetX - entity.position.x;
        if (Mathf.Abs(directionX) > 0.1f)
        {
            entity.localScale = new Vector3(directionX > 0 ? -1 : 1, 1, 1);
        }

        while (Mathf.Abs(entity.position.x - targetX) > 0.1f)
        {
            float newX = Mathf.MoveTowards(entity.position.x, targetX, speed * Time.deltaTime);
            entity.position = new Vector3(newX, entity.position.y, entity.position.z);
            yield return null;
        }

        if (anim != null) anim.SetBool("isRunning", false);
    }

    private IEnumerator PlayDialogueSequence(LBYH_Line[] lines)
    {
        if (dialogueUI == null) 
        {
            Debug.LogError("<color=red>Dialogue UI is missing in Scene6Master! Please drag your DialogueBox into the inspector slot!</color>");
            yield break;
        }
        
        // Freeze Player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        KnightHero hero = null;
        if (playerObj != null)
        {
            hero = playerObj.GetComponent<KnightHero>();
            if (hero != null) hero.enabled = false;
            Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            
            Animator anim = playerObj.GetComponent<Animator>();
            if (anim != null) anim.Play("KnightIdle");
        }

        foreach (var line in lines)
        {
            dialogueUI.PresentLine(line);
            yield return new WaitForEndOfFrame();
            while (dialogueUI.IsTyping) yield return null;
            
            // Wait for Advance
            yield return new WaitUntil(() =>
                Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0));
            
            yield return new WaitUntil(() =>
                !Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.Return) &&
                !Input.GetKey(KeyCode.E) && !Input.GetMouseButton(0));
                
            yield return new WaitForSeconds(0.1f);
        }
        
        dialogueUI.HideDialoguePanel();
        if (hero != null) hero.enabled = true; // Unfreeze
    }

    bool IsBossDefeated()
    {
        if (janitorBoss == null) return true;
        EnemyHealth h = janitorBoss.GetComponent<EnemyHealth>();
        return (h != null && h.currentHealth <= 0) || !janitorBoss.activeInHierarchy;
    }
}
