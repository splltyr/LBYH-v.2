using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Scene6SequenceController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private LBYH_Dialogue dialogue;

    [Header("Phase 1: The Intro & Rescue")]
    [SerializeField] private LBYH_Line[] introLines = new LBYH_Line[]
    {
        /* 0 */ new LBYH_Line { name = "Yves", text = "Ugh, How's my luck worse than this?" },
        /* 1 */ new LBYH_Line { name = "Tala", text = "Good thing that I don't have legs." },
        /* 2 */ new LBYH_Line { name = "Tala", text = "Yves 0, Tala 1. Muehehehe" },
        /* 3 */ new LBYH_Line { name = "Yves", text = "I didn't know that your name is Tala." },
        /* 4 */ new LBYH_Line { name = "Tala", text = "Well, I never really got the chance to introduce myself." },
        /* 5 */ new LBYH_Line { name = "Yves", text = "Hmm.. Who exactly are you?" },
        /* 6 */ new LBYH_Line { name = "Tala", text = "Glad you asked, even if it's late. Haloo! I am Tala, I am a spirit that roams around here in this world." },
        /* 7 */ new LBYH_Line { name = "Yves", text = "Purpose, huh? Your purpose is to be a glowy bubble that chatters a lot?" },
        /* 8 */ new LBYH_Line { name = "Tala", text = "HEY! But really, my purpose is to guide y-" },
        /* 9 */ new LBYH_Line { name = "Narrative", text = "[Some Sneaks and Mechanics have appeared. Suddenly a male voice calls for help.]" },
        /* 10 */ new LBYH_Line { name = "Male Voice", text = "I swear on every deity. If you don- AGH!" },
        /* 11 */ new LBYH_Line { name = "Tala", text = "*sighs* Aaand my words get cut off again.. That way, Yves!" },
        /* 12 */ new LBYH_Line { name = "Yves", text = "Let's go!" }
    };

    [Header("Phase 2: Meeting Sir Dave")]
    [SerializeField] private LBYH_Line[] meetDaveLines = new LBYH_Line[]
    {
        /* 13 */ new LBYH_Line { name = "Yves", text = "Are you hurt?" },
        /* 14 */ new LBYH_Line { name = "???", text = "What deity are you to save me, young man?" },
        /* 15 */ new LBYH_Line { name = "Tala", text = "Wow, Yves? A deity? He's funny." },
        /* 16 */ new LBYH_Line { name = "Yves", text = "Uh.. I am no deity, Sir..?" },
        /* 17 */ new LBYH_Line { name = "Sir Dave", text = "Ah, apologies.. My name is Dave, I was once a Professor here on STI." },
        /* 18 */ new LBYH_Line { name = "Yves", text = "Sir Dave? Oh, Sir Rome told me that you might be able to help me.." },
        /* 19 */ new LBYH_Line { name = "Sir Dave", text = "Correct. I can upgrade your weapons and make you even stronger." },
        /* 20 */ new LBYH_Line { name = "Tala", text = "That would be awesome!" },
        /* 21 */ new LBYH_Line { name = "Sir Dave", text = "May I ask if Reya and Rome are doing fine?" },
        /* 22 */ new LBYH_Line { name = "Yves", text = "Yes, Sir Dave. I made sure that they're safe before coming here." },
        /* 23 */ new LBYH_Line { name = "Sir Dave", text = "So that would mean that you have defeated the Corrupted's, yes?" },
        /* 24 */ new LBYH_Line { name = "Yves", text = "Uh.. Yes sir." },
        /* 25 */ new LBYH_Line { name = "Tala", text = "It's almost like an interview.." },
        /* 26 */ new LBYH_Line { name = "Sir Dave", text = "Let me see what I can do for you." },
        /* 27 */ new LBYH_Line { name = "Narrative", text = "[Sir Dave prepares his tools. Yves upgrades his weapons.]" }
    };

    [Header("Phase 3: The Janitor Boss Spawn")]
    [SerializeField] private LBYH_Line[] bossSpawnLines = new LBYH_Line[]
    {
        /* 28 */ new LBYH_Line { name = "Narrative", text = "[The lights flicker. The sewer water ripples violently.]" },
        /* 29 */ new LBYH_Line { name = "Yves", text = "That doesn't smell good." },
        /* 30 */ new LBYH_Line { name = "Tala", text = "That doesn't look good." },
        /* 31 */ new LBYH_Line { name = "Sir Dave", text = "Kid... get ready. That's not just movement... that's the boss!" },
        /* 32 */ new LBYH_Line { name = "Corrupted Janitor", text = "CLLLEEEAAANNN... EVERTHING MUST BE CLEAN... FOR MASTER [REDACTED]!" },
        /* 33 */ new LBYH_Line { name = "Yves", text = "Yeah... definitely corrupted... A Weird looking ONE!?" },
        /* 34 */ new LBYH_Line { name = "Tala", text = "Be careful! He charges fast—wait for the right moment!" }
    };

    [Header("Phase 4: Boss Banter & Ending")]
    [SerializeField] private LBYH_Line[] bossEndLines = new LBYH_Line[]
    {
        /* 35 */ new LBYH_Line { name = "Janitor", text = "W-Where's my d-daughter.. Aghh! it hurts.." },
        /* 36 */ new LBYH_Line { name = "Yves", text = "Sir Dave, please help!" },
        /* 37 */ new LBYH_Line { name = "Sir Dave", text = "I'll take him somewhere safe. You keep moving, kid." },
        /* 38 */ new LBYH_Line { name = "Yves", text = "Hey Tala, how come this one had a person inside?" },
        /* 39 */ new LBYH_Line { name = "Tala", text = "His body was corrupted, but his soul was pure. Let's focus on what's ahead." }
    };

    [Header("Combat References")]
    [SerializeField] private GameObject firstWaveEnemies;
    [SerializeField] private Transform firstWaveSpawnPoint;
    [SerializeField] private GameObject janitorBoss;
    [SerializeField] private Transform bossSpawnPoint;

    enum Phase { Intro, FightingWave1, MeetDave, BossSpawn, FightingBoss, Ending, Done }
    Phase currentPhase = Phase.Intro;

    void Start()
    {
        if (dialogue == null) dialogue = FindAnyObjectByType<LBYH_Dialogue>();
        if (firstWaveEnemies != null) firstWaveEnemies.SetActive(false);
        if (janitorBoss != null) janitorBoss.SetActive(false);

        StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        // 1. Intro Dialogue
        currentPhase = Phase.Intro;
        yield return PlayDialogue(introLines, 9); // Spawn enemies at index 9

        // 2. Wait for Wave 1 to die
        currentPhase = Phase.FightingWave1;
        Debug.Log("Waiting for first wave to be defeated...");
        yield return new WaitUntil(() => IsDead(firstWaveEnemies));

        // 3. Meet Dave
        currentPhase = Phase.MeetDave;
        yield return PlayDialogue(meetDaveLines);

        // 4. Boss Spawn Dialogue
        currentPhase = Phase.BossSpawn;
        yield return PlayDialogue(bossSpawnLines, 32); // Spawn boss at "CLEAN" line

        // 5. Wait for Boss to die
        currentPhase = Phase.FightingBoss;
        Debug.Log("Waiting for Janitor Boss to be defeated...");
        yield return new WaitUntil(() => IsDead(janitorBoss));

        // 6. Ending Dialogue
        currentPhase = Phase.Ending;
        yield return PlayDialogue(bossEndLines);

        currentPhase = Phase.Done;
        Debug.Log("Scene 6 Sequence Complete!");
    }

    IEnumerator PlayDialogue(LBYH_Line[] lines, int spawnIndex = -1)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            dialogue.PresentLine(lines[i]);
            
            // Check for Spawns
            if (spawnIndex != -1 && i == (spawnIndex % 100)) // Hacky check for local vs global index
            {
                TriggerSpawn(currentPhase);
            }

            while (dialogue.IsTyping) yield return null;
            yield return WaitForAdvance();
        }
        dialogue.HideDialoguePanel();
    }

    void TriggerSpawn(Phase phase)
    {
        if (phase == Phase.Intro && firstWaveEnemies != null)
        {
            if (firstWaveSpawnPoint != null) firstWaveEnemies.transform.position = firstWaveSpawnPoint.position;
            firstWaveEnemies.SetActive(true);
        }
        else if (phase == Phase.BossSpawn && janitorBoss != null)
        {
            if (bossSpawnPoint != null) janitorBoss.transform.position = bossSpawnPoint.position;
            janitorBoss.SetActive(true);
        }
    }

    bool IsDead(GameObject obj)
    {
        if (obj == null) return true;
        if (!obj.activeSelf) return false; // Not spawned yet
        
        // Find ALL health components in children
        var eHealths = obj.GetComponentsInChildren<EnemyHealth>(true);
        var bHealths = obj.GetComponentsInChildren<BossHealth>(true);

        // If no health components found, assume it's dead (or not an enemy)
        if (eHealths.Length == 0 && bHealths.Length == 0) return true;

        // Check regular enemies
        foreach (var h in eHealths)
        {
            if (h != null && h.currentHealth > 0f) return false;
        }

        // Check boss-type enemies
        foreach (var h in bHealths)
        {
            if (h != null && h.health > 0) return false;
        }

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
