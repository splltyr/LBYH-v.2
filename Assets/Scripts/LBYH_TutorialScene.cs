using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LBYH_TutorialScene : MonoBehaviour
{
    [Header("Transition Settings")]
    [Tooltip("Type the exact name of the first level scene here! (e.g. 'Scene 1')")]
    [SerializeField] private string nextSceneName = "Scene 1"; 

    [Tooltip("Optional black screen CanvasGroup for a smooth fade out transition.")]
    [SerializeField] private CanvasGroup fadeScreen;
    [SerializeField] private float fadeDuration = 1f;

    [Header("UI References")]
    public TMPro.TextMeshProUGUI tutorialText;
    public float typingSpeed = 0.04f;

    [Header("Audio Settings")]
    [Tooltip("Voice slot for the 'how to play...' introduction.")]
    public AudioClip tutorialVoice;
    [Tooltip("Optional voice slot for the 'run to the LEFT' instruction.")]
    public AudioClip runLeftVoice;
    public AudioSource audioSource;

    private bool isTransitioning = false;
    private int currentStep = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        // Auto-acquire components if unassigned, avoiding the Unity ?? operator gotcha
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (tutorialText == null)
        {
            tutorialText = FindAnyObjectByType<TMPro.TextMeshProUGUI>();
        }

        Debug.Log($"<color=yellow>[Tutorial Debug] Start initiated. AudioSource: {audioSource}, TextMeshPro: {tutorialText}</color>");

        // Smoothly fade in from black at the start of the tutorial (if assigned)
        if (fadeScreen != null)
        {
            fadeScreen.alpha = 1f;
            StartCoroutine(FadeIn());
        }

        // Start the tutorial sequence
        StartCoroutine(ShowStep(0));
    }

    void Update()
    {
        if (isTransitioning) return;

        // Press Space, Enter, E, or Left Click to advance or skip typing!
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // Skip typewriter animation and show full text instantly
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                isTyping = false;
                if (tutorialText != null)
                {
                    if (currentStep == 0)
                    {
                        tutorialText.text = "HOW TO PLAY\n\n" +
                                             "[ PC CONTROLS ]\n" +
                                             "- Move Left / Right: A / D\n" +
                                             "- Jump: SPACE\n" +
                                             "- Attack: Left Click\n" +
                                             "- Dash: Left Shift\n\n" +
                                             "[ MOBILE CONTROLS ]\n" +
                                             "- Use Virtual Joystick & Buttons to Move, Jump, Attack, and Dash!\n\n" +
                                             "<size=24><color=#aaaaaa>[ Press Space or Click to continue ]</color></size>";
                    }
                    else if (currentStep == 1)
                    {
                        tutorialText.text = "Please, do your best to run to the LEFT.\n\n<size=24><color=#aaaaaa>[ Press Space or Click to Start! ]</color></size>";
                    }
                }
            }
            else
            {
                if (currentStep == 0)
                {
                    StartCoroutine(ShowStep(1));
                }
                else
                {
                    StartCoroutine(LoadNextScene());
                }
            }
        }
    }

    IEnumerator ShowStep(int step)
    {
        currentStep = step;
        isTyping = true;

        string targetText = "";
        AudioClip voiceClip = null;

        if (step == 0)
        {
            targetText = "HOW TO PLAY\n\n" +
                         "[ PC CONTROLS ]\n" +
                         "- Move Left / Right: A / D\n" +
                         "- Jump: SPACE\n" +
                         "- Attack: Left Click\n" +
                         "- Dash: Left Shift\n\n" +
                         "[ MOBILE CONTROLS ]\n" +
                         "- Use Virtual Joystick & Buttons to Move, Jump, Attack, and Dash!";
            voiceClip = tutorialVoice;
        }
        else if (step == 1)
        {
            targetText = "Please, do your best to run to the LEFT.";
            voiceClip = runLeftVoice;
        }

        Debug.Log($"<color=cyan>[Tutorial Debug] ShowStep({step}) called. VoiceClip: {(voiceClip != null ? voiceClip.name : "NULL")}</color>");

        if (voiceClip != null && audioSource != null)
        {
            audioSource.clip = voiceClip;
            audioSource.Play();
            Debug.Log($"<color=green>[Tutorial Debug] AudioSource.Play() executed for: {voiceClip.name}</color>");
        }

        if (tutorialText != null)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(targetText, step));
        }
        else
        {
            isTyping = false;
        }
        yield break;
    }

    IEnumerator TypeText(string text, int step)
    {
        tutorialText.text = "";
        foreach (char c in text)
        {
            tutorialText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        if (step == 0)
        {
            tutorialText.text += "\n\n<size=24><color=#aaaaaa>[ Press Space or Click to continue ]</color></size>";
        }
        else
        {
            tutorialText.text += "\n\n<size=24><color=#aaaaaa>[ Press Space or Click to Start! ]</color></size>";
        }
        isTyping = false;
    }

    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (fadeScreen != null) fadeScreen.alpha = 1f - (elapsed / fadeDuration);
            yield return null;
        }
        if (fadeScreen != null) fadeScreen.alpha = 0f;
    }

    IEnumerator LoadNextScene()
    {
        isTransitioning = true;

        // Smooth fade to black
        if (fadeScreen != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeScreen.alpha = elapsed / fadeDuration;
                yield return null;
            }
            fadeScreen.alpha = 1f;
        }

        Debug.Log($"[Tutorial] Loading first level: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }
}
