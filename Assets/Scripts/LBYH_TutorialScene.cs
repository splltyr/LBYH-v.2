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

    private bool isTransitioning = false;

    void Start()
    {
        // Smoothly fade in from black at the start of the tutorial (if assigned)
        if (fadeScreen != null)
        {
            fadeScreen.alpha = 1f;
            StartCoroutine(FadeIn());
        }
    }

    void Update()
    {
        if (isTransitioning) return;

        // Press Space, Enter, E, or Left Click to start the game!
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
        {
            StartCoroutine(LoadNextScene());
        }
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
