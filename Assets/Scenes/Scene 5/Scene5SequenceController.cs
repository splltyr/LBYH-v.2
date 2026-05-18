using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene5SequenceController : MonoBehaviour
{
    [Header("UI & Systems")]
    [SerializeField] private LBYH_Dialogue dialogue;
    [SerializeField] private LBYH_CircuitPuzzle puzzle;
    [SerializeField] private GameObject fadeOverlay;
    [SerializeField] private string nextSceneName = "Scene 6";

    [Header("Character References")]
    [SerializeField] private KnightHero player;

    [Header("World References")]
    [SerializeField] private GameObject sneaksGroup;
    [SerializeField] private GameObject puzzleTabletTrigger;
    [SerializeField] private GameObject doorTeleportTrigger;
    [SerializeField] private Transform bullyArenaTeleportPoint;
    [SerializeField] private GameObject bullyBoss; // The actual boss in the arena

    [Header("Dialogue Content")]
    private LBYH_Line[] introLines = {
        new LBYH_Line { name = "Yves", text = "Ugh why is this floor so creepy.. And dang it stinks. . ." },
        new LBYH_Line { name = "Tala", text = "Jeez.. The smell makes me wanna disappear." },
        new LBYH_Line { name = "Yves", text = "Aren’t you a spirit?" },
        new LBYH_Line { name = "Tala", text = "Oh.." }
    };

    private LBYH_Line[] ambushLines = {
        new LBYH_Line { name = "Tala", text = "YVES! THE CEILING! WATCH OU-" },
        new LBYH_Line { name = "Yves", text = "Huh..?" }
    };

    private LBYH_Line[] postAmbushLines = {
        new LBYH_Line { name = "Yves", text = "So this is what Sir Rome is talking about? What is it called… Ah, Sneaks. " },
        new LBYH_Line { name = "Tala", text = "Good job, Yves!" },
        new LBYH_Line { name = "Tala", text = "Look, there! That's the Lab." }
    };

    private LBYH_Line[] doorLockedLines = {
        new LBYH_Line { name = "Yves", text = "Yeah.. I don’t think there’s a key for this floor." }
    };

    private LBYH_Line[] noteLines = {
        new LBYH_Line { name = "Narrative", text = "“Beware of the gate… If you solve this Pathfinding Maze you may seek the floor you go to but there will be a Guardian…”" },
        new LBYH_Line { name = "Tala", text = "There’s a Guardian now?!" },
        new LBYH_Line { name = "Yves", text = "I thought you knew everything about this place?" },
        new LBYH_Line { name = "Tala", text = "Uh.. No, I didn't.. " }
    };

    private LBYH_Line[] postNoteLines = {
        new LBYH_Line { name = "Tala", text = "Yves, take a look!" },
        new LBYH_Line { name = "Yves", text = "No other options, I must go forward to it." }
    };

    private LBYH_Line[] postPuzzleLines = {
        new LBYH_Line { name = "Tala", text = "Do you know that you’re bad at this?" },
        new LBYH_Line { name = "Yves", text = "I’m no expert, I did my best! That took me about 30 minutes! " },
        new LBYH_Line { name = "Tala", text = "Now let’s see what’s in here." }
    };

    private LBYH_Line[] bullyIntroLines = {
        new LBYH_Line { name = "Yves", text = "What the…" },
        new LBYH_Line { name = "Tala", text = "That doesn’t look good.." },
        new LBYH_Line { name = "Tala", text = "Yves! On your left!" },
        new LBYH_Line { name = "Yves", text = "So you're the one who keeps the corrupted in bay." },
        new LBYH_Line { name = "Yves", text = "A…Bully?" }
    };

    private LBYH_Line[] bullyMidFightLines = {
        new LBYH_Line { name = "School Bully", text = "Clever, Brat. But you won’t be going up and saving the others of how many My corrupted digitals are forming when I kill you!" },
        new LBYH_Line { name = "Yves", text = "I won’t let you HURT THEM!" },
        new LBYH_Line { name = "Tala", text = "Yeah! Yves will protect them, so get defeated!" }
    };

    private LBYH_Line[] bullyMidFight2Lines = {
        new LBYH_Line { name = "School Bully", text = "Stop acting like a brat!" },
        new LBYH_Line { name = "Tala", text = "Is he.. talking to a mirror or something..?" }
    };

    private LBYH_Line[] victoryLines = {
        new LBYH_Line { name = "Yves", text = "Who’s a brat now?" },
        new LBYH_Line { name = "Tala", text = "If only our guidance counselors were here.. They will never let this idiot in." },
        new LBYH_Line { name = "Yves", text = "Hey, that’s not nice." },
        new LBYH_Line { name = "Tala", text = "Hmph! You look terrible." },
        new LBYH_Line { name = "Yves", text = "I’ll wipe it off. Look, there’s an elevator." }
    };

    private bool puzzleCompleted = false;
    private bool nearPuzzleTablet = false;
    private bool nearDoor = false;
    private bool midFight1Triggered = false;
    private bool midFight2Triggered = false;

    IEnumerator Start()
    {
        if (player == null) player = FindAnyObjectByType<KnightHero>();
        if (dialogue == null) dialogue = FindAnyObjectByType<LBYH_Dialogue>();
        if (puzzle == null) puzzle = FindAnyObjectByType<LBYH_CircuitPuzzle>();

        if (sneaksGroup != null) sneaksGroup.SetActive(false);
        
        // Keep the door visible but disable its interaction/collider at the start
        if (doorTeleportTrigger != null) 
        {
            Scene5DoorZone dz = doorTeleportTrigger.GetComponent<Scene5DoorZone>();
            if (dz != null) dz.SetEnabled(false);
            else
            {
                Collider2D col = doorTeleportTrigger.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
            }
        }
        if (bullyBoss != null) bullyBoss.SetActive(false);

        yield return new WaitForSeconds(1f);
        StartCoroutine(SceneSequence());
    }

    IEnumerator SceneSequence()
    {
        // 1. Intro
        Debug.Log("<color=cyan>Scene5 Phase: Intro Dialogue</color>");
        SetPlayerControl(false);
        yield return PlayDialogue(introLines);

        // 2. Ambush Trigger
        Debug.Log("<color=cyan>Scene5 Phase: Ambush Starting</color>");
        yield return PlayDialogue(ambushLines);
        if (sneaksGroup != null) sneaksGroup.SetActive(true);
        SetPlayerControl(true);

        // 3. Wait for Sneaks Death
        Debug.Log("<color=cyan>Scene5 Phase: Waiting for all Sneaks to die...</color>");
        yield return new WaitUntil(() => IsDead(sneaksGroup));
        
        // 4. Post-Sneaks
        Debug.Log("<color=cyan>Scene5 Phase: Post-Ambush Dialogue</color>");
        SetPlayerControl(false);
        yield return PlayDialogue(postAmbushLines);
        
        // 5. Enable Teleport Immediately
        puzzleCompleted = true; // Mark as completed to bypass trigger locks
        nearDoor = false; 
        if (doorTeleportTrigger != null) 
        {
            Scene5DoorZone dz = doorTeleportTrigger.GetComponent<Scene5DoorZone>();
            if (dz != null) dz.SetEnabled(true);
            else
            {
                Collider2D col = doorTeleportTrigger.GetComponent<Collider2D>();
                if (col != null) col.enabled = true;
            }
        }
        SetPlayerControl(true);
        Debug.Log("<color=green>Scene5: Gate is unlocked immediately after ambush! Player can go to the door.</color>");

        // 6. Wait for Teleport to Boss Arena (Player walks into the door trigger)
        yield return new WaitUntil(() => nearDoor);
        
        // Directly perform the teleportation!
        if (player != null && bullyArenaTeleportPoint != null)
        {
            player.transform.position = bullyArenaTeleportPoint.position;
            Debug.Log("<color=green>Scene5: Teleported player to Bully Arena!</color>");
        }
        
        SetPlayerControl(false);
        if (bullyBoss != null) 
        {
            // The boss is placed at X = 102.4 in the editor, while the teleport point is at X = 57.13 (45 units away!)
            // We dynamically reposition the boss 8 units to the right of the player so he actually shows up on-screen!
            float bossY = bullyArenaTeleportPoint.position.y + 0.96f; // Maintain original vertical offset
            bullyBoss.transform.position = new Vector3(bullyArenaTeleportPoint.position.x + 8f, bossY, 0f);
            bullyBoss.SetActive(true);
        }
        yield return PlayDialogue(bullyIntroLines);
        SetPlayerControl(true);

        // 11. Boss Fight Loops
        StartCoroutine(HandleBossDialogueTriggers());
        yield return new WaitUntil(() => IsDead(bullyBoss));

        // 12. Victory
        SetPlayerControl(false);
        yield return PlayDialogue(victoryLines);

        // 13. End Scene
        if (fadeOverlay != null) fadeOverlay.SetActive(true);
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(nextSceneName); // Customizable scene transition!
    }

    IEnumerator HandleBossDialogueTriggers()
    {
        if (bullyBoss == null) yield break;
        EnemyHealth health = bullyBoss.GetComponent<EnemyHealth>();
        if (health == null) yield break;

        while (health.currentHealth > 0)
        {
            if (!midFight1Triggered && health.currentHealth < health.maxHealth * 0.7f)
            {
                midFight1Triggered = true;
                yield return PlayDialogue(bullyMidFightLines);
            }

            if (!midFight2Triggered && health.currentHealth < health.maxHealth * 0.3f)
            {
                midFight2Triggered = true;
                yield return PlayDialogue(bullyMidFight2Lines);
            }
            yield return null;
        }
    }

    IEnumerator PlayDialogue(LBYH_Line[] lines)
    {
        if (lines == null || lines.Length == 0) yield break;
        for (int i = 0; i < lines.Length; i++)
        {
            LBYH_Line line = lines[i];
            if (line.name != "Narrative" && !string.IsNullOrEmpty(line.name))
                line.text = line.name + ": " + line.text;

            dialogue.PresentLine(line);
            while (dialogue.IsTyping) yield return null;
            yield return WaitForInput();
        }
        dialogue.HideDialoguePanel();
    }

    IEnumerator WaitForInput()
    {
        yield return null;
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));
    }

    void SetPlayerControl(bool state)
    {
        if (player != null)
        {
            player.enabled = state;
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            Animator anim = player.GetComponentInChildren<Animator>();
            if (!state && anim != null) anim.Play("KnightIdle");
        }
    }

    bool IsDead(GameObject obj)
    {
        if (obj == null) return true;
        if (!obj.activeInHierarchy) return true;
        
        EnemyHealth eh = obj.GetComponent<EnemyHealth>();
        if (eh != null) return eh.currentHealth <= 0;
        
        // If it's a group, check children
        EnemyHealth[] children = obj.GetComponentsInChildren<EnemyHealth>();
        foreach (var c in children) if (c.currentHealth > 0) return false;
        
        return true;
    }

    // Trigger Hooks for Unity
    public void SetNearTablet(bool state) { nearPuzzleTablet = state; }
    public void SetNearDoor(bool state) { nearDoor = state; }
    
    // OLD COMPATIBILITY: Fixes the compiler error in Scene5DoorZone
    public void NotifyPlayerReachedDoor() { nearDoor = true; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == puzzleTabletTrigger) nearPuzzleTablet = true;
        if (collision.gameObject == doorTeleportTrigger && !puzzleCompleted) Debug.Log("Door is locked until puzzle is solved.");
        if (collision.gameObject == doorTeleportTrigger && puzzleCompleted)
        {
             player.transform.position = bullyArenaTeleportPoint.position;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == puzzleTabletTrigger) nearPuzzleTablet = false;
    }
}
