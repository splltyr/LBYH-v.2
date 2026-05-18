using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Shows LBYH_Line rows (name, text, optional voice). Add this to your dialogue box GameObject (e.g. DialogueBox_0).</summary>
public class LBYH_Dialogue : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI textDisplay;
    public TextMeshProUGUI nameDisplay;
    [Tooltip("If set, Show/Hide uses this object. Otherwise uses textDisplay's parent.")]
    public GameObject dialoguePanel;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Typewriter Settings")]
    public float typeSpeed = 0.03f;

    [Header("Puzzle Integration")]
    public LBYH_CircuitPuzzle circuitPuzzle;

    public bool IsVisible 
    { 
        get 
        { 
            var root = PanelRoot; 
            return root != null && root.activeInHierarchy; 
        } 
    }

    public bool IsTyping => typingCoroutine != null;
    private Coroutine typingCoroutine;

    readonly Queue<LBYH_Line> lines = new Queue<LBYH_Line>();

    public static LBYH_Dialogue Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (audioSource != null) audioSource.playOnAwake = false;

        if (GetComponent<AutoVolumeNormalizer>() == null) gameObject.AddComponent<AutoVolumeNormalizer>();

        // Auto-find puzzle if not assigned
        if (circuitPuzzle == null)
            circuitPuzzle = FindAnyObjectByType<LBYH_CircuitPuzzle>(FindObjectsInactive.Include);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    GameObject PanelRoot
    {
        get
        {
            if (dialoguePanel != null) return dialoguePanel;
            if (textDisplay != null && textDisplay.transform.parent != null)
                return textDisplay.transform.parent.gameObject;
            return null;
        }
    }

    public void StartDialogue(LBYH_Line[] dialogueGroup)
    {
        lines.Clear();
        if (dialogueGroup == null) return;
        foreach (var line in dialogueGroup)
            lines.Enqueue(line);
        DisplayNext();
    }

    public void DisplayNext()
    {
        if (lines.Count == 0)
        {
            HideDialoguePanel();
            return;
        }

        LBYH_Line current = lines.Dequeue();
        PresentLine(current);
    }

    private string fullText;
    private bool skipTyping = false;

    public void PresentLine(LBYH_Line line)
    {
        if (line == null) return;
        var root = PanelRoot;
        if (root != null) root.SetActive(true);
        if (nameDisplay != null) nameDisplay.text = line.name;

        fullText = line.text;
        skipTyping = false;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(fullText));

        if (audioSource != null)
        {
            audioSource.Stop();
            
            if (line.voiceClip != null)
            {
                // BULLETPROOF VOLUME BOOST HACK (Global)
                int playCount = Mathf.Max(1, Mathf.CeilToInt(line.volume));
                float volumePerClip = line.volume / playCount;

                for (int i = 0; i < playCount; i++)
                {
                    audioSource.PlayOneShot(line.voiceClip, volumePerClip);
                }
            }
        }
    }

    void Update()
    {
        // NEVER steal input while the circuit puzzle is open —
        // clicks belong to the puzzle tiles, not the dialogue.
        if (circuitPuzzle != null && circuitPuzzle.isPuzzleOpen) return;

        // If typing and player presses an advance key, skip to end
        if (IsTyping && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E)))
        {
            skipTyping = true;
        }
    }

    IEnumerator TypeText(string text)
    {
        if (textDisplay == null)
        {
            typingCoroutine = null;
            yield break;
        }

        textDisplay.text = "";
        foreach (char c in text)
        {
            if (skipTyping)
            {
                textDisplay.text = text;
                break;
            }
            textDisplay.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        typingCoroutine = null;
        skipTyping = false;
    }

    public void HideDialoguePanel()
    {
        var root = PanelRect();
        if (root != null) root.SetActive(false);
    }

    private GameObject PanelRect()
    {
        if (dialoguePanel != null) return dialoguePanel;
        if (textDisplay != null && textDisplay.transform.parent != null)
            return textDisplay.transform.parent.gameObject;
        return null;
    }
}
