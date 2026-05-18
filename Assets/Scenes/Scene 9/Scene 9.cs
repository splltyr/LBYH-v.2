using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Scene9 : MonoBehaviour
{
    [Header("UI & Dialogue")]
    [SerializeField] private LBYH_Dialogue dialogue;
    [SerializeField] private LibraryPuzzleUI puzzleUI;

    [Header("Combat References")]
    [SerializeField] private GameObject smallFriesGroup; // Skeletons and Sneaks
    [SerializeField] private GameObject reaperBoss;
    [SerializeField] private GameObject tableInteractable; // The table Yves needs to find
    [SerializeField] private GameObject exitDoor;

    [Header("Camera Control")]
    [SerializeField] private Transform mainCamera; // Or Cinemachine Virtual Camera

    [Header("Audio & Music")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip normalBGM;
    [SerializeField] private AudioClip bossBGM;

    [Header("Dialogue Content")]
    private LBYH_Line[] introLines = {
        new LBYH_Line { name = "Tala", text = "I’M TIRED!" },
        new LBYH_Line { name = "Yves", text = "Just one more floor.." },
        new LBYH_Line { name = "Yves", text = "By the way, these floating books are awesome." },
        new LBYH_Line { name = "Tala", text = "Not professional." },
        new LBYH_Line { name = "Yves", text = "This library is too big. How am I gonna find the final source code to stop the corruption?" },
        new LBYH_Line { name = "Tala", text = "Don’t worry, I’ll be here when you don’t understand things. I am sure you can do it, have some faith." }
    };

    private LBYH_Line[] postPuzzleLines = {
        new LBYH_Line { name = "Yves", text = "I think I found it!" },
        new LBYH_Line { name = "Tala", text = "Oooh~ Great job, Yves! You found the source code keycard!" },
        new LBYH_Line { name = "Yves", text = "Good thing I have keen eyes, hehe." },
        new LBYH_Line { name = "Tala", text = "Is that.. A Reaper?" },
        new LBYH_Line { name = "Tala", text = "Oh yeah, I forgot about that." },
        new LBYH_Line { name = "Yves", text = "Wait.. WHAT..?" },
        new LBYH_Line { name = "Tala", text = "HEHEHE, Goodluck!" }
    };

    private LBYH_Line[] victoryLines = {
        new LBYH_Line { name = "Yves", text = "If only I could boil you, Tala." },
        new LBYH_Line { name = "Tala", text = "Guess what? You can’t, for as I am a spirit." },
        new LBYH_Line { name = "Yves", text = "…" }
    };

    private bool puzzleCompleted = false;
    private bool nearTable = false;
    public static Scene9 Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        // Auto-find puzzleUI if it wasn't assigned in the inspector
        if (puzzleUI == null) puzzleUI = Object.FindAnyObjectByType<LibraryPuzzleUI>();
        Debug.Log($"<color=white>Scene9: Instance Ready. PuzzleUI found: {puzzleUI != null}</color>");
    }

    void Start()
    {
        if (dialogue == null) dialogue = FindAnyObjectByType<LBYH_Dialogue>();
        if (puzzleUI == null) puzzleUI = FindAnyObjectByType<LibraryPuzzleUI>();
        if (mainCamera == null && Camera.main != null) mainCamera = Camera.main.transform;

        if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;

        // Initialize state
        if (reaperBoss != null) reaperBoss.SetActive(false);
        if (tableInteractable != null) tableInteractable.SetActive(false);
        if (exitDoor != null) exitDoor.SetActive(false);

        StartCoroutine(SceneSequence());
    }

    IEnumerator SceneSequence()
    {
        if (bgmSource != null && normalBGM != null) { bgmSource.clip = normalBGM; bgmSource.Play(); }

        // 1. INTRO DIALOGUE
        yield return PlayDialogue(introLines);

        // 2. WAIT FOR ENEMIES TO DIE
        Debug.Log("Waiting for small fries to be defeated...");
        yield return new WaitUntil(() => IsDead(smallFriesGroup));
        
        // Wait a bit so they don't just "poof" instantly
        yield return new WaitForSeconds(1.5f); 


        // 3. POST-COMBAT DIALOGUE & TABLE REVEAL
        // Lines: "I think I found it!", "Great job Yves!", "Good thing I have keen eyes"
        yield return PlayPartialDialogue(postPuzzleLines, 0, 2); 
        
        if (tableInteractable != null) tableInteractable.SetActive(true);
        Debug.Log("Enemies cleared! Dialogue played. Find the table.");

        // 4. WAIT FOR PUZZLE INTERACTION
        yield return new WaitUntil(() => puzzleCompleted);

        // 5. BOSS REVEAL DIALOGUE
        // Lines: "Is that.. A Reaper?", "Oh yeah, I forgot about that."
        yield return PlayPartialDialogue(postPuzzleLines, 3, 4);

        // 6. CAMERA HOVER TO BOSS
        if (bgmSource != null && bossBGM != null) { bgmSource.clip = bossBGM; bgmSource.Play(); }
        
        if (reaperBoss != null) reaperBoss.SetActive(true);
        yield return CameraFocusOn(reaperBoss.transform, 3f);

        // 7. FINISH BOSS INTRO DIALOGUE
        // Lines: "Wait.. WHAT..?", "HEHEHE, Goodluck!"
        yield return PlayPartialDialogue(postPuzzleLines, 5, 6);

        // 8. BOSS FIGHT
        yield return new WaitUntil(() => IsDead(reaperBoss));

        // 9. VICTORY & OUTRO
        yield return PlayDialogue(victoryLines);

        // 10. SHOW EXIT
        if (exitDoor != null) exitDoor.SetActive(true);
        Debug.Log("Scene 9 Complete! Go to the door.");
    }

    void Update()
    {
        // DEBUG: Press 'P' to force open the puzzle for testing!
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("<color=yellow>DEBUG: Force Opening Puzzle!</color>");
            puzzleUI.OpenPuzzle();
            puzzleUI.onPuzzleComplete = () => { puzzleCompleted = true; };
        }

        // Handle 'E' or Auto-Open to interact with table
        if (!puzzleCompleted && nearTable)
        {
            // We already auto-open in SetNearTable, but we can keep this for safety
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("<color=cyan>Scene9: Manual E Interaction!</color>");
                puzzleUI.OpenPuzzle();
                puzzleUI.onPuzzleComplete = () => { puzzleCompleted = true; };
            }
        }
    }

    // --- HELPER FUNCTIONS ---

    private IEnumerator PlayDialogue(LBYH_Line[] lines)
    {
        if (dialogue == null) dialogue = Object.FindAnyObjectByType<LBYH_Dialogue>();
        if (dialogue == null) { Debug.LogError("Scene9: NO DIALOGUE FOUND!"); yield break; }

        foreach (var line in lines)
        {
            Debug.Log($"<color=white>Dialogue: {line.name} says '{line.text}'</color>");
            dialogue.PresentLine(line);
            
            yield return new WaitForSeconds(0.2f); // Small pause before allowing skip
            while (dialogue.IsTyping) yield return null;
            
            Debug.Log("<color=white>Dialogue: Waiting for Advance Key...</color>");
            yield return WaitForAdvance();
        }
        dialogue.HideDialoguePanel();
    }

    private IEnumerator PlayPartialDialogue(LBYH_Line[] lines, int start, int end)
    {
        if (dialogue == null) yield break;

        for (int i = start; i <= end; i++)
        {
            dialogue.PresentLine(lines[i]);
            yield return new WaitForEndOfFrame();
            while (dialogue.IsTyping) yield return null;
            yield return WaitForAdvance();
        }
        dialogue.HideDialoguePanel();
    }

    IEnumerator CameraFocusOn(Transform target, float duration)
    {
        Vector3 originalPos = mainCamera.position;
        float elapsed = 0f;
        float moveTime = 1f;

        // Move to target
        while (elapsed < moveTime)
        {
            mainCamera.position = Vector3.Lerp(originalPos, new Vector3(target.position.x, target.position.y, originalPos.z), elapsed / moveTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(duration);

        // Move back
        elapsed = 0f;
        while (elapsed < moveTime)
        {
            mainCamera.position = Vector3.Lerp(mainCamera.position, originalPos, elapsed / moveTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mainCamera.position = originalPos;
    }

    IEnumerator WaitForAdvance()
    {
        // Wait for a fresh press
        yield return new WaitUntil(() =>
            Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0));
        
        // Wait for the button to be RELEASED so it doesn't trigger the next line immediately
        yield return new WaitUntil(() =>
            !Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.Return) &&
            !Input.GetKey(KeyCode.E) && !Input.GetMouseButton(0));
        
        yield return new WaitForSeconds(0.1f); // Tiny buffer
    }

    bool IsDead(GameObject obj)
    {
        if (obj == null) return true;
        EnemyHealth[] healths = obj.GetComponentsInChildren<EnemyHealth>(true);
        BossHealth[] bossHealths = obj.GetComponentsInChildren<BossHealth>(true);

        foreach (var h in healths) if (h != null && h.currentHealth > 0) return false;
        foreach (var h in bossHealths) if (h != null && h.health > 0) return false;

        return true;
    }

    public void SetNearTable(bool state) 
    { 
        nearTable = state; 
        if (state) 
        {
            // Check if small fries are still alive
            if (!IsDead(smallFriesGroup))
            {
                Debug.Log("<color=orange>Scene9: Puzzle blocked! You must defeat the enemies first.</color>");
                return;
            }

            Debug.Log("<color=green>Scene9: TRIGGER DETECTED! Opening Puzzle...</color>");
            if (!puzzleCompleted)
            {
                if (puzzleUI != null) 
                {
                    puzzleUI.OpenPuzzle();
                    puzzleUI.onPuzzleComplete = () => { 
                        Debug.Log("<color=yellow>Scene9: Puzzle Logic Complete!</color>");
                        puzzleCompleted = true; 
                    };
                }
                else Debug.LogError("Scene9: CANNOT OPEN! PuzzleUI is null in Inspector.");
            }
        }
        else 
        {
            Debug.Log("<color=orange>Scene9: Player LEFT the trigger area.</color>");
        }
    }
}
