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

    void Start()
    {
        if (dialogueUI == null) dialogueUI = FindAnyObjectByType<LBYH_Dialogue>();
        if (reaperEnemy != null) reaperEnemy.SetActive(false);
        if (puzzleSystem != null) puzzleSystem.gameObject.SetActive(false);
        
        StartCoroutine(RunLibrarySequence());
    }

    IEnumerator RunLibrarySequence()
    {
        // 1. Arrival
        yield return PlayBlock(introBlock);

        // 2. The Puzzle (Maze & Word Question)
        if (puzzleSystem != null)
        {
            puzzleSystem.gameObject.SetActive(true);
            yield return new WaitUntil(() => puzzleSystem.isPuzzleComplete);
        }

        // 3. Finding the Code
        yield return PlayBlock(findingCodeBlock);

        // 4. The Reaper Appears
        if (reaperEnemy != null) reaperEnemy.SetActive(true);
        yield return PlayBlock(reaperBlock);

        Debug.Log("Scene 9 Complete! Proceed to Penthouse.");
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
