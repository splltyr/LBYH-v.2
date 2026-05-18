using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene9SequenceController : MonoBehaviour
{
    [Header("Dialogue Blocks")]
    [SerializeField] private LBYH_DialogueData introBlock;
    [SerializeField] private LBYH_DialogueData findingCodeBlock;
    [SerializeField] private LBYH_DialogueData reaperBlock;

    [Header("UI & Systems")]
    [SerializeField] private LBYH_Dialogue dialogueUI;
    [SerializeField] private LibraryPuzzleController puzzleSystem;
    [SerializeField] private GameObject reaperEnemy;
    [SerializeField] private AudioSource voiceSource;

    [Header("Transitions & Effects")]
    [SerializeField] private CanvasGroup screenFader;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private GameObject objectiveArrows;

    private PlayerMovement player;

    void Start()
    {
        player = FindAnyObjectByType<PlayerMovement>();
        if (dialogueUI == null) dialogueUI = FindAnyObjectByType<LBYH_Dialogue>();
        if (reaperEnemy != null) reaperEnemy.SetActive(false);
        if (puzzleSystem != null) puzzleSystem.gameObject.SetActive(false);
        if (objectiveArrows != null) objectiveArrows.SetActive(false);
        
        // Ensure we start with a clear screen if the fader is used for Fade IN elsewhere, 
        // but here we primarily use it for the final Fade OUT.
        if (screenFader != null) screenFader.alpha = 0;

        StartCoroutine(RunLibrarySequence());
    }

    IEnumerator RunLibrarySequence()
    {
        // 1. Arrival (Intro Block)
        if (player != null) player.SetSpeed(0);
        yield return PlayBlock(introBlock);
        if (player != null) player.SetSpeed(10f);

        // 2. The Puzzle (Maze & Word Question)
        // Yves focuses, arrows point him to the puzzle
        if (objectiveArrows != null) objectiveArrows.SetActive(true);
        
        if (puzzleSystem != null)
        {
            puzzleSystem.gameObject.SetActive(true);
            yield return new WaitUntil(() => puzzleSystem.isPuzzleComplete);
        }

        // Hide arrows once puzzle is done
        if (objectiveArrows != null) objectiveArrows.SetActive(false);

        // 3. Finding the Code
        if (player != null) player.SetSpeed(0);
        yield return PlayBlock(findingCodeBlock);
        if (player != null) player.SetSpeed(10f);

        // 4. The Reaper Appears
        if (player != null) player.SetSpeed(0);
        if (reaperEnemy != null) reaperEnemy.SetActive(true);
        yield return PlayBlock(reaperBlock);

        // Final: Screen Fades
        Debug.Log("Scene 9 Complete! Fading out...");
        yield return StartCoroutine(Fade(1f));

        // Logic to load next scene could go here
        Debug.Log("Proceed to Penthouse.");
    }

    IEnumerator Fade(float targetAlpha)
    {
        if (screenFader == null) yield break;
        float startAlpha = screenFader.alpha;
        float time = 0;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            screenFader.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }
        screenFader.alpha = targetAlpha;
    }

    IEnumerator PlayBlock(LBYH_DialogueData block)
    {
        if (block == null) yield break;
        foreach (var line in block.lines)
        {
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

    IEnumerator WaitForInput()
    {
        yield return null;
        yield return new WaitUntil(() =>
            Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E));
    }
}
