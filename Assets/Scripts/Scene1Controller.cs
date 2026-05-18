using UnityEngine;
using TMPro;

public class Scene1Controller : MonoBehaviour
{
    [Header("Tutorial UI")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;

    [Header("Boss Dialogue")]
    [TextArea(3, 10)] public string bossDialogue = "[REDACTED]: You still think this is YOUR school, Yves? It's all just a simulation of failure, and you're just a bug in the code.";
    public AudioClip bossVoiceClip;
    public AudioSource voiceAudioSource;

    [Header("Teleport Settings")]
    public string nextSceneName = "Scene 2 Clinic"; // Where Damon teleports you

    void Start()
    {
        // DESTROY any rogue teleport triggers so only Damon handles the teleport!
        foreach (var t in FindObjectsByType<SceneTransitionTrigger>(FindObjectsSortMode.None)) Destroy(t.gameObject);
        foreach (var t in FindObjectsByType<LevelLoader>(FindObjectsSortMode.None)) Destroy(t.gameObject);
        foreach (var t in FindObjectsByType<UniversalScenePortal>(FindObjectsSortMode.None)) Destroy(t.gameObject);
    }

    public void TriggerBossDialogue()
    {
        // For story dialogue, we DON'T pause the game
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);

            // Hide other UI junk
            foreach (Transform child in tutorialPanel.transform)
            {
                if (tutorialText != null && child == tutorialText.transform) child.gameObject.SetActive(true);
                else child.gameObject.SetActive(false); // Hide Continue button for story
            }

            if (tutorialText != null)
            {
                TypewriterEffect tw = tutorialText.GetComponent<TypewriterEffect>();
                if (tw != null) tw.ShowText(bossDialogue);
                else tutorialText.text = bossDialogue;
            }

            if (bossVoiceClip != null)
            {
                if (voiceAudioSource == null) 
                {
                    voiceAudioSource = gameObject.AddComponent<AudioSource>();
                    voiceAudioSource.playOnAwake = false;
                    gameObject.AddComponent<AutoVolumeNormalizer>();
                }
                voiceAudioSource.playOnAwake = false; // Force it off in case it was added manually in the inspector
                voiceAudioSource.PlayOneShot(bossVoiceClip);
            }

            // Auto-hide the box and teleport after 5 seconds so it doesn't stay forever
            Invoke(nameof(HideAndTeleport), 5f);
        }
    }

    private void HideAndTeleport()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        
        Debug.Log("<color=cyan>Scene 1: Damon's dialogue finished! Teleporting to " + nextSceneName + "!</color>");
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }

    // --- EMPTY STUBS --- 
    // These prevent Unity from throwing errors if the invisible triggers are still in the scene!
    public void TriggerJumpTutorial() {}
    public void TriggerSlideTutorial() {}
    public void ShowTutorial(string message) {}
    public bool IsIntroDone() => true;
}
