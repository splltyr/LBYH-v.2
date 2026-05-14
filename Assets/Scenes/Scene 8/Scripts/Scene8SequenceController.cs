using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scene8SequenceController : MonoBehaviour
{
    [Header("Dialogue Blocks")]
    [SerializeField] private LBYH_DialogueData introBlock;
    [SerializeField] private LBYH_DialogueData ambushBlock;
    [SerializeField] private LBYH_DialogueData questioningBlock;
    [SerializeField] private LBYH_DialogueData flashbackBlock;
    [SerializeField] private LBYH_DialogueData endingBlock;

    [Header("UI References")]
    [SerializeField] private LBYH_Dialogue dialogueUI;
    [SerializeField] private Image screenOverlay; 
    [SerializeField] private AudioSource voiceAudioSource;

    [Header("Scene Objects")]
    [SerializeField] private GameObject fileFlingersGroup;
    [SerializeField] private List<LaserTrap> lasers;

    void Start()
    {
        if (dialogueUI == null) dialogueUI = FindAnyObjectByType<LBYH_Dialogue>();
        if (voiceAudioSource == null) voiceAudioSource = gameObject.AddComponent<AudioSource>();
        
        if (fileFlingersGroup != null) fileFlingersGroup.SetActive(false);
        
        StartCoroutine(RunSceneSequence());
    }

    IEnumerator RunSceneSequence()
    {
        // 1. Intro
        yield return PlayBlock(introBlock);

        // 2. Ambush
        if (fileFlingersGroup != null) fileFlingersGroup.SetActive(true);
        yield return PlayBlock(ambushBlock);
        
        // Give time for combat visuals
        yield return new WaitForSeconds(1.5f); 

        // 3. Questions
        yield return PlayBlock(questioningBlock);

        // 4. Flashback
        yield return StartCoroutine(FlashbackTransition(true));
        yield return PlayBlock(flashbackBlock);
        yield return StartCoroutine(FlashbackTransition(false));

        // 5. Final Revelation
        yield return PlayBlock(endingBlock);
        
        Debug.Log("Scene 8 Narrative Complete.");
    }

    IEnumerator PlayBlock(LBYH_DialogueData block)
    {
        if (block == null) yield break;

        foreach (var line in block.lines)
        {
            // Play voice if present
            if (line.voiceClip != null && voiceAudioSource != null)
            {
                voiceAudioSource.clip = line.voiceClip;
                voiceAudioSource.Play();
            }

            dialogueUI.PresentLine(line);
            while (dialogueUI.IsTyping) yield return null;
            yield return WaitForInput();
        }
        dialogueUI.HideDialoguePanel();
    }

    IEnumerator FlashbackTransition(bool active)
    {
        if (screenOverlay == null) yield break;
        
        float targetAlpha = active ? 0.6f : 0f;
        Color targetColor = active ? new Color(0.7f, 0.4f, 0.1f, 0.6f) : new Color(0,0,0,0);
        float startAlpha = screenOverlay.color.a;

        for (float t = 0; t < 1f; t += Time.deltaTime)
        {
            Color c = Color.Lerp(screenOverlay.color, targetColor, t);
            screenOverlay.color = c;
            yield return null;
        }
    }

    IEnumerator WaitForInput()
    {
        yield return null;
        yield return new WaitUntil(() =>
            Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E));
    }
}
