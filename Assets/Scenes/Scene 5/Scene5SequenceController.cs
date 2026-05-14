using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Scene5SequenceController : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private LBYH_Dialogue dialogue;
    [SerializeField] private GameObject dialogueObject;

    [Header("1 — Opening")]
    [SerializeField] private LBYH_DialogueData openingDialogueData;
    [SerializeField] private LBYH_Line[] openingLines;
    [SerializeField] private int spawnSneaksAtLineIndex = 4;

    [Header("2 — Sneaks")]
    [SerializeField] private GameObject sneaksTemplate;
    [SerializeField] private Transform sneaksSpawnPoint;

    [Header("3 — After Sneaks dies")]
    [SerializeField] private LBYH_DialogueData afterSneaksDialogueData;

    [SerializeField] private LBYH_Line[] dialogueAfterSneaksDeath = new LBYH_Line[]
    {
        /* 0  */ new LBYH_Line { name = "Yves",        text = "Ugh why is this floor so creepy.. And dang it stinks. . ." },
        /* 1  */ new LBYH_Line { name = "Tala",        text = "Jeez.. The smell makes me wanna disappear." },
        /* 2  */ new LBYH_Line { name = "Yves",        text = "Aren't you a spirit?" },
        /* 3  */ new LBYH_Line { name = "Tala",        text = "Oh.." },
        /* 4  */ new LBYH_Line { name = "Tala",        text = "Look, there! That's the Lab." },
        /* 5  */ new LBYH_Line { name = "Narrative",   text = "[Yves walks to the double door but is locked. Yves smashed the door with his weapon.]" },
        /* 6  */ new LBYH_Line { name = "Yves",        text = "Yeah.. I don't think there's a key for this floor." },
        /* 7  */ new LBYH_Line { name = "Narrative",   text = "[Yves looked and wandered around the abandoned laboratory, then saw a circuit board with multiple color wires and a Note.]" },
        /* 8  */ new LBYH_Line { name = "Yves & Tala", text = "\"Beware of the gate... If you solve this Pathfinding Maze you may seek the floor you go to but there will be a Guardian...\"" },
        /* 9  */ new LBYH_Line { name = "Tala",        text = "There's a Guardian now?!" },
        /* 10 */ new LBYH_Line { name = "Yves",        text = "I thought you knew everything about this place?" },
        /* 11 */ new LBYH_Line { name = "Tala",        text = "Uh.. No, I didn't.." },
        /* 12 */ new LBYH_Line { name = "Narrative",   text = "[After Yves and Tala finish the note, Tala then looks at the circuit board.]" },
        /* 13 */ new LBYH_Line { name = "Tala",        text = "Yves, take a look!" },
        /* 14 */ new LBYH_Line { name = "Yves",        text = "No other options, I must go forward to it." },
        /* 15 */ new LBYH_Line { name = "Tala",        text = "Do you know that you're bad at this?" },
        /* 16 */ new LBYH_Line { name = "Yves",        text = "I'm no expert, I did my best! That took me about 30 minutes!" },
        /* 17 */ new LBYH_Line { name = "Tala",        text = "Now let's see what's in here." },
        /* 18 */ new LBYH_Line { name = "Tala",        text = "...Whoa. Let's go, Yves." },
    };

    [Tooltip("The puzzle will trigger at index 10 ('I thought you knew...')")]
    [SerializeField] private int triggerMazeAtLineIndex = 10;
    
    [SerializeField] private LBYH_CircuitPuzzle mazePuzzle;
    [SerializeField] private int spawnBullyAtLineIndex = 99;
    [SerializeField] private GameObject bullyTemplate;
    [SerializeField] private Transform bullySpawnPoint;

    [Header("4 — Door & Teleport")]
    public Scene5DoorZone doorZone;
    public Transform bullyArenaTeleportPoint;
    public UnityEvent onPartOneComplete;

    enum Phase { Boot, Opening, SneaksFight, PostSneaks, WaitingDoor, Done }
    Phase phase = Phase.Boot;
    GameObject spawnedSneaks;
    Component[] sneaksHealths; // Changed to Component to support multiple health types
    GameObject spawnedBully;

    // Forces index to 10 in Unity Editor
    void OnValidate() { triggerMazeAtLineIndex = 10; }

    void Awake()
    {
        if (dialogue == null && dialogueObject != null)
            dialogue = dialogueObject.GetComponent<LBYH_Dialogue>();
        if (dialogue == null)
            dialogue = FindAnyObjectByType<LBYH_Dialogue>(FindObjectsInactive.Include);

        if (mazePuzzle == null)
            mazePuzzle = FindAnyObjectByType<LBYH_CircuitPuzzle>(FindObjectsInactive.Include);
    }

    void Start()
    {
        if (doorZone != null) doorZone.Initialize(this);
        if (mazePuzzle != null && mazePuzzle.puzzleRoot != null)
            mazePuzzle.puzzleRoot.SetActive(false);

        phase = Phase.Opening;
        StartCoroutine(RunOpeningThenFight());
    }

    IEnumerator RunOpeningThenFight()
    {
        LBYH_Line[] openLines = ResolveLines(openingDialogueData, openingLines);
        if (openLines == null || openLines.Length == 0)
        {
            phase = Phase.SneaksFight;
            yield return RunSneaksFight();
            yield break;
        }

        for (int i = 0; i < openLines.Length; i++)
        {
            dialogue.PresentLine(openLines[i]);
            if (i == spawnSneaksAtLineIndex) SpawnSneaksIfNeeded();
            while (dialogue.IsTyping) yield return null;
            yield return WaitForDialogueAdvance();
        }

        dialogue.HideDialoguePanel();
        phase = Phase.SneaksFight;
        yield return RunSneaksFight();
    }

    IEnumerator RunSneaksFight()
    {
        if (sneaksHealths == null || sneaksHealths.Length == 0) SpawnSneaksIfNeeded();
        if (sneaksHealths != null && sneaksHealths.Length > 0)
        {
            yield return new WaitUntil(() => AllHealthsDead(sneaksHealths));
        }
        phase = Phase.PostSneaks;
        yield return RunPostSneaksDialogue();
    }

    bool AllHealthsDead(Component[] healths)
    {
        if (healths == null || healths.Length == 0) return true;
        foreach (var h in healths)
        {
            if (h == null) continue;

            if (h is EnemyHealth eh && eh.currentHealth > 0f) return false;
            if (h is BossHealth bh && bh.health > 0) return false;
        }
        return true;
    }

    IEnumerator RunPostSneaksDialogue()
    {
        LBYH_Line[] postLines = ResolveLines(afterSneaksDialogueData, dialogueAfterSneaksDeath);
        
        if (afterSneaksDialogueData != null)
            Debug.LogWarning("USING ASSET FOR DIALOGUE: Check the asset indexes if the puzzle doesn't trigger!");

        if (postLines == null || postLines.Length == 0)
        {
            EnterWaitingDoor();
            yield break;
        }

        for (int i = 0; i < postLines.Length; i++)
        {
            Debug.Log($"<color=white>Dialogue Step: {i} - Speaker: {postLines[i].name}</color>");
            bool linePresented = false;

            if (i == triggerMazeAtLineIndex)
            {
                Debug.Log("<color=magenta>PUZZLE TRIGGER REACHED AT INDEX " + i + "!</color>");
                if (mazePuzzle == null) mazePuzzle = FindAnyObjectByType<LBYH_CircuitPuzzle>();
                
                dialogue.PresentLine(postLines[i]);
                linePresented = true;
                while (dialogue.IsTyping) yield return null;
                yield return WaitForDialogueAdvance();

                dialogue.HideDialoguePanel();
                if (mazePuzzle != null)
                {
                    mazePuzzle.OpenPuzzle();
                    yield return new WaitUntil(() => mazePuzzle.isSolved);
                    Debug.Log("<color=green>PUZZLE SOLVED - CONTINUING DIALOGUE</color>");
                }
                else
                {
                    Debug.LogError("MAZE PUZZLE SCRIPT NOT FOUND!");
                }
            }

            if (i == spawnBullyAtLineIndex) SpawnBullyIfNeeded();

            if (!linePresented)
            {
                dialogue.PresentLine(postLines[i]);
                while (dialogue.IsTyping) yield return null;
                yield return WaitForDialogueAdvance();
            }
        }

        dialogue.HideDialoguePanel();
        EnterWaitingDoor();
    }

    void EnterWaitingDoor()
    {
        phase = Phase.WaitingDoor;
        if (doorZone != null) doorZone.SetEnabled(true);
        else onPartOneComplete?.Invoke();
    }

    public void NotifyPlayerReachedDoor()
    {
        if (phase != Phase.WaitingDoor) return;

        if (bullyArenaTeleportPoint != null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) player = FindAnyObjectByType<KnightHero>()?.gameObject;

            if (player != null)
            {
                player.transform.position = bullyArenaTeleportPoint.position;
                LBYH_Line[] postLines = ResolveLines(afterSneaksDialogueData, dialogueAfterSneaksDeath);
                if (postLines != null && postLines.Length > 18)
                {
                    dialogue.PresentLine(postLines[18]);
                    StartCoroutine(HideDialogueAfterAdvance());
                }
            }
        }

        phase = Phase.Done;
        if (doorZone != null) doorZone.SetEnabled(false);
        onPartOneComplete?.Invoke();
    }

    IEnumerator HideDialogueAfterAdvance()
    {
        while (dialogue.IsTyping) yield return null;
        yield return WaitForDialogueAdvance();
        dialogue.HideDialoguePanel();
    }

    void SpawnSneaksIfNeeded()
    {
        if (spawnedSneaks != null || sneaksTemplate == null || sneaksSpawnPoint == null) return;
        if (!sneaksTemplate.scene.IsValid()) spawnedSneaks = Instantiate(sneaksTemplate, sneaksSpawnPoint.position, sneaksSpawnPoint.rotation);
        else { sneaksTemplate.transform.SetPositionAndRotation(sneaksSpawnPoint.position, sneaksSpawnPoint.rotation); sneaksTemplate.SetActive(true); spawnedSneaks = sneaksTemplate; }
        
        // Find all health components (both regular and boss)
        var eHealths = spawnedSneaks.GetComponentsInChildren<EnemyHealth>(true);
        var bHealths = spawnedSneaks.GetComponentsInChildren<BossHealth>(true);
        
        List<Component> combined = new List<Component>();
        combined.AddRange(eHealths);
        combined.AddRange(bHealths);
        sneaksHealths = combined.ToArray();

        Debug.Log($"<color=cyan>Scene5: Found {sneaksHealths.Length} enemies in the spawned group.</color>");
    }

    void SpawnBullyIfNeeded()
    {
        if (spawnedBully != null || bullyTemplate == null || bullySpawnPoint == null) return;
        spawnedBully = Instantiate(bullyTemplate, bullySpawnPoint.position, bullySpawnPoint.rotation);
    }

    static LBYH_Line[] ResolveLines(LBYH_DialogueData data, LBYH_Line[] inline)
    {
        if (data != null && data.lines != null && data.lines.Length > 0) return data.lines;
        return inline;
    }

    static IEnumerator WaitForDialogueAdvance()
    {
        yield return null;
        yield return new WaitUntil(() =>
            Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0));
    }
}
