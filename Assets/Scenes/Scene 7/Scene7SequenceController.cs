using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Scene7SequenceController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private LBYH_Dialogue dialogue;

    [Header("Phase 1: Intro")]
    [SerializeField] private LBYH_Line[] introLines = new LBYH_Line[]
    {
        new LBYH_Line { name = "Tala", text = "This place.. I remember things as if they were just yesterday." },
        new LBYH_Line { name = "Yves", text = "Have you been on this floor?" },
        new LBYH_Line { name = "Tala", text = "More than you can imagine." },
        new LBYH_Line { name = "Narrative", text = "[Glitch enemies begin forming from computer parts.]" },
        new LBYH_Line { name = "Yves", text = "So this is where things get worse…" }
    };

    [Header("Phase 2: After First Fight")]
    [SerializeField] private LBYH_Line[] postFirstFightLines = new LBYH_Line[]
    {
        new LBYH_Line { name = "Yves", text = "That was tough.. Let’s get moving, Tala." },
        new LBYH_Line { name = "Tala", text = "May you guys rest, in peace." }
    };

    [Header("Phase 3: MIS Arrival")]
    [SerializeField] private LBYH_Line[] misArrivalLines = new LBYH_Line[]
    {
        new LBYH_Line { name = "Narrative", text = "[Yves starts walking, eventually arriving at the front of the MIS.]" },
        new LBYH_Line { name = "Male Voice", text = "AGHH! S-STOP! I CAN’T SEE YOU GUYS LIKE THAT!" },
        new LBYH_Line { name = "Yves", text = "It's coming from inside the MIS!" },
        new LBYH_Line { name = "Tala", text = "That voice.. Yves, quick!" },
        new LBYH_Line { name = "Narrative", text = "[Yves kicks down the door of the MIS and spots a person surrounded by enemies.]" },
        new LBYH_Line { name = "Yves", text = "Hey! Over here!" },
        new LBYH_Line { name = "Tala", text = "Be careful, Yves.. Something’s wrong with them." }
    };

    [Header("Phase 4: Glitch Dialogue")]
    [SerializeField] private LBYH_Line[] glitchRescueLines = new LBYH_Line[]
    {
        new LBYH_Line { name = "Yves", text = "I defeated them all, they look different from the others but I managed." },
        new LBYH_Line { name = "Glitch", text = "N-no, I-im okay t-thank you so much f-for the help. B-By the w-way, my n-name is G-Glitch." },
        new LBYH_Line { name = "Tala", text = "So, it is him.." },
        new LBYH_Line { name = "Yves", text = "Woah! That is such a cool name, by the way my name is Yves, I'm glad I came here and was able to rescue you." },
        new LBYH_Line { name = "Glitch", text = "I-I w-was stuck in t-this place b-because I was working w-when s-suddenly the e-enemies attacked and they look o-oddly f-familiar.. " },
        new LBYH_Line { name = "Glitch", text = "I-I couldn't get o-out of t-this r-room, and t-then suddenly c-creatures c-came out of the c-computer monitors and s-started t-to corner m-me, I thought I-I was g-gonna d-die right t-then and there!" },
        new LBYH_Line { name = "Yves", text = "You’re safe now, Glitch!" },
        new LBYH_Line { name = "Glitch", text = "T-thank you.. P-Please l-let me help y-you i-in return of saving m-me, I-I can s-supply y-you with armors t-to i-increase your d-defense." },
        new LBYH_Line { name = "Yves", text = "WOAH! That would be cool, Glitch! But given your situation right now, you need to go. There are people who can help you on the ground floor." },
        new LBYH_Line { name = "Glitch", text = "O-Okay Y-Yves, T-thank you s-so much for s-saving me, h-here h-have this." },
        new LBYH_Line { name = "Glitch", text = "(FORESHADOWING) J-just i-in case, I-I think y-you will n-need it." },
        new LBYH_Line { name = "Yves", text = "I really appreciate it. Thank you, Glitch." }
    };

    [Header("Phase 5: Post Rescue & Boss")]
    [SerializeField] private LBYH_Line[] postRescueLines = new LBYH_Line[]
    {
        new LBYH_Line { name = "Narrative", text = "[Yves walks out of the MIS.]" },
        new LBYH_Line { name = "Yves", text = "Tala? You’ve been awfully quiet.. Are you well?" },
        new LBYH_Line { name = "Tala", text = "Just- Yves! Watch out!" },
        new LBYH_Line { name = "Narrative", text = "[The boss MALWARE attacks! Yves is protected by his new armor.]" },
        new LBYH_Line { name = "Yves", text = "I’m really glad that glitch gave me this armor, otherwise I would’ve been badly injured.." },
        new LBYH_Line { name = "Tala", text = "Works like a charm." },
        new LBYH_Line { name = "Yves", text = "Now, where were we?" }
    };

    [Header("Phase 6: Final")]
    [SerializeField] private LBYH_Line[] finalLines = new LBYH_Line[]
    {
        new LBYH_Line { name = "Yves", text = "(Tired, gasping for air) There… I took care of that monster…. Now, I better get to the next floor and see what's up there." },
        new LBYH_Line { name = "Tala", text = "You look like a mess." },
        new LBYH_Line { name = "Yves", text = "You’re lucky I can’t touch you." },
        new LBYH_Line { name = "Tala", text = "Scary.." }
    };

    [Header("Combat References")]
    [SerializeField] private GameObject firstWaveEnemies;
    [SerializeField] private GameObject misWaveEnemies;
    [SerializeField] private GameObject malwareBoss;
    [SerializeField] private Transform elevatorDoor;
    [SerializeField] private Transform misDoor;
    [SerializeField] private List<LaserTrap> lasers; // Added laser references

    enum Phase { Intro, Fight1, MovingToMIS, KickDoor, GlitchDialogue, FightBoss, FinalDialogue, Done }
    Phase currentPhase = Phase.Intro;

    void Start()
    {
        if (dialogue == null) dialogue = FindAnyObjectByType<LBYH_Dialogue>();
        
        // Hide all enemies at start
        if (firstWaveEnemies != null) firstWaveEnemies.SetActive(false);
        if (misWaveEnemies != null) misWaveEnemies.SetActive(false);
        if (malwareBoss != null) malwareBoss.SetActive(false);

        // Optional: Start with lasers on
        SetLasersActive(true);

        StartCoroutine(RunSequence());
    }

    void SetLasersActive(bool active)
    {
        if (lasers == null) return;
        foreach (var laser in lasers)
        {
            if (laser != null) laser.enabled = active;
        }
    }

    IEnumerator RunSequence()
    {
        // 1. Intro
        currentPhase = Phase.Intro;
        yield return PlayDialogue(introLines);
        
        // 2. Spawn & Fight Wave 1
        currentPhase = Phase.Fight1;
        if (firstWaveEnemies != null) firstWaveEnemies.SetActive(true);
        yield return new WaitUntil(() => IsDead(firstWaveEnemies));
        yield return PlayDialogue(postFirstFightLines);

        // 3. Move to MIS
        currentPhase = Phase.MovingToMIS;
        yield return PlayDialogue(misArrivalLines);
        
        // 4. Kick door and fight MIS enemies
        currentPhase = Phase.KickDoor;
        if (misDoor != null) misDoor.gameObject.SetActive(false); // "Kick down" door
        if (misWaveEnemies != null) misWaveEnemies.SetActive(true);
        yield return new WaitUntil(() => IsDead(misWaveEnemies));

        // 5. Dialogue with Glitch
        currentPhase = Phase.GlitchDialogue;
        yield return PlayDialogue(glitchRescueLines);

        // 6. Boss attack
        currentPhase = Phase.FightBoss;
        yield return PlayDialogue(postRescueLines);
        if (malwareBoss != null) malwareBoss.SetActive(true);
        yield return new WaitUntil(() => IsDead(malwareBoss));

        // 7. Ending
        currentPhase = Phase.FinalDialogue;
        yield return PlayDialogue(finalLines);

        currentPhase = Phase.Done;
        Debug.Log("Scene 7 Sequence Complete!");
    }

    IEnumerator PlayDialogue(LBYH_Line[] lines)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            dialogue.PresentLine(lines[i]);
            while (dialogue.IsTyping) yield return null;
            yield return WaitForAdvance();
        }
        dialogue.HideDialoguePanel();
    }

    bool IsDead(GameObject obj)
    {
        if (obj == null) return true;
        if (!obj.activeSelf) return false;
        
        var eHealths = obj.GetComponentsInChildren<EnemyHealth>(true);
        var bHealths = obj.GetComponentsInChildren<BossHealth>(true);

        if (eHealths.Length == 0 && bHealths.Length == 0) return true;

        foreach (var h in eHealths) if (h != null && h.currentHealth > 0f) return false;
        foreach (var h in bHealths) if (h != null && h.health > 0) return false;

        return true;
    }

    IEnumerator WaitForAdvance()
    {
        yield return null;
        yield return new WaitUntil(() =>
            Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0));
    }
}
