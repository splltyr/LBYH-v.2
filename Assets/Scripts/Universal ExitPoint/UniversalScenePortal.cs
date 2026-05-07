using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class UniversalScenePortal : MonoBehaviour
{
    [Header("Travel Settings")]
    public string targetSceneName; 

    [Header("Visuals")]
    public CanvasGroup screenFade; 
    public float fadeDuration = 1f;

    private bool isTransitioning = false;

    private void Start()
    {
        // 1. Automatically Fade IN when the scene starts
        if (screenFade != null)
        {
            // Ensure it's fully black first
            screenFade.alpha = 1;
            StartCoroutine(Fade(0)); 
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check for Yves (tagged as Player)
        if (collision.CompareTag("Player") && !isTransitioning)
        {
            StartCoroutine(TransitionToScene());
        }
    }

    private IEnumerator TransitionToScene()
    {
        isTransitioning = true;

        // 2. Fade OUT before loading
        if (screenFade != null)
        {
            yield return StartCoroutine(Fade(1));
        }

        // 3. Load the next map
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }

    // A universal Fade function to handle both directions
    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = screenFade.alpha;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            screenFade.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        screenFade.alpha = targetAlpha;
    }
}