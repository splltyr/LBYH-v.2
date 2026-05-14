using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LibraryPuzzleController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button[] answerButtons;
    
    [Header("Puzzle Data")]
    public string question = "What is the core protocol that handles data packet addressing and routing?";
    public string[] answers = { "TCP", "UDP", "IP", "HTTP" };
    public int correctAnswerIndex = 2; // "IP"

    public bool isPuzzleComplete { get; private set; }

    void Start()
    {
        if (quizPanel != null) quizPanel.SetActive(false);
        SetupQuiz();
    }

    public void ActivateQuiz()
    {
        if (quizPanel != null) quizPanel.SetActive(true);
    }

    void SetupQuiz()
    {
        if (questionText != null) questionText.text = question;
        
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < answers.Length)
            {
                int index = i;
                answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = answers[i];
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
            }
        }
    }

    void OnAnswerSelected(int index)
    {
        if (index == correctAnswerIndex)
        {
            Debug.Log("<color=green>Correct Answer!</color>");
            isPuzzleComplete = true;
            if (quizPanel != null) quizPanel.SetActive(false);
        }
        else
        {
            Debug.Log("<color=red>Wrong Answer! Try again.</color>");
            // Optional: Shake screen or play fail sound
        }
    }
}
