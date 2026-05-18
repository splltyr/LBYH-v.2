using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro; // Added for TextMeshPro support

[Serializable]
public class Riddle
{
    public string question;
    public string[] answers = new string[4];
    public int correctAnswerIndex;
}

public class LibraryPuzzleUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject puzzlePanel;        // The blue box
    public TextMeshProUGUI questionText;  // Changed to TextMeshProUGUI
    public Button[] answerButtons;       // Your 4 buttons

    [Header("Riddles Content")]
    public List<Riddle> riddles = new List<Riddle>();
    private int currentRiddleIndex = 0;

    public Action onPuzzleComplete;

    void Awake()
    {
        // AUTOMATIC SETUP
        if (puzzlePanel == null) puzzlePanel = transform.Find("PuzzleDialogue")?.gameObject;
        if (puzzlePanel == null) puzzlePanel = gameObject;

        Debug.Log($"<color=white>PuzzleUI: Initializing on {gameObject.name}. Panel: {puzzlePanel.name}</color>");

        if (questionText == null) questionText = puzzlePanel.GetComponentInChildren<TextMeshProUGUI>();
        if (questionText == null) Debug.LogError("PuzzleUI: Could not find Question Text (TMP)!");

        // Find and Setup Buttons
        if (answerButtons == null || answerButtons.Length == 0)
        {
            answerButtons = puzzlePanel.GetComponentsInChildren<Button>();
            Debug.Log($"PuzzleUI: Found {answerButtons.Length} buttons.");
        }

        if (answerButtons.Length == 0) Debug.LogError("PuzzleUI: Could not find any Buttons!");

        // Setup button texts and listeners
        for (int i = 0; i < answerButtons.Length; i++)
        {
            TextMeshProUGUI tmpText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            int index = i; 
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => SelectAnswer(index));
            
            if (tmpText == null) Debug.LogWarning($"PuzzleUI: Button {i} has no TMP text!");
        }
    }

    void Start()
    {
        puzzlePanel.SetActive(false);
        SetupDefaultRiddles();
    }

    public void OpenPuzzle()
    {
        Debug.Log("<color=white>PuzzleUI: OpenPuzzle called! Activating Panel.</color>");
        puzzlePanel.SetActive(true);
        currentRiddleIndex = 0;
        ShowRiddle();
    }

    void ShowRiddle()
    {
        if (currentRiddleIndex >= riddles.Count) return;

        Riddle current = riddles[currentRiddleIndex];
        if (questionText != null) questionText.text = $"Question {currentRiddleIndex + 1}/3:\n{current.question}";

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < current.answers.Length)
            {
                // Support both TextMeshPro and Legacy Text for buttons
                TextMeshProUGUI tmpText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (tmpText != null) 
                {
                    tmpText.text = current.answers[i];
                }
                else 
                {
                    Text legacyText = answerButtons[i].GetComponentInChildren<Text>();
                    if (legacyText != null) legacyText.text = current.answers[i];
                }
            }
        }
    }

    public void SelectAnswer(int index)
    {
        if (currentRiddleIndex < riddles.Count && index == riddles[currentRiddleIndex].correctAnswerIndex)
        {
            Debug.Log("<color=green>Correct!</color>");
            currentRiddleIndex++;
            if (currentRiddleIndex >= riddles.Count) FinishPuzzle();
            else ShowRiddle();
        }
        else
        {
            Debug.Log("<color=red>Wrong answer!</color>");
        }
    }

    void FinishPuzzle()
    {
        Debug.Log("<color=gold>Puzzle Complete!</color>");
        puzzlePanel.SetActive(false);
        onPuzzleComplete?.Invoke();
    }

    void SetupDefaultRiddles()
    {
        if (riddles.Count > 0) return;
        riddles.Add(new Riddle { question = "I have keys, but no locks. I have a space, but no room. You can enter, but never leave. What am I?", answers = new string[] { "Map", "Keyboard", "House", "Car" }, correctAnswerIndex = 1 });
        riddles.Add(new Riddle { question = "What has a thumb and four fingers, but is not alive?", answers = new string[] { "Shadow", "Glove", "Statue", "Mirror" }, correctAnswerIndex = 1 });
        riddles.Add(new Riddle { question = "The more of this there is, the less you see. What is it?", answers = new string[] { "Light", "Fog", "Darkness", "Knowledge" }, correctAnswerIndex = 2 });
    }
}
