using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LBYH_Credits : MonoBehaviour
{
    [Header("Scrolling Settings")]
    [Tooltip("Drag and drop your UI Text or standard GameObject directly from the Hierarchy window into this slot!")]
    [SerializeField] private Transform creditsTextTransform;
    [SerializeField] private float scrollSpeed = 80f;
    
    [Tooltip("How many seconds should the credits scroll before automatically fading out and loading the next scene?")]
    [SerializeField] private float scrollDuration = 15f; 

    [Header("Transition Settings")]
    [SerializeField] private string nextSceneName = "MainMenu";
    [SerializeField] private CanvasGroup fadeScreen;
    [SerializeField] private float fadeDuration = 2f;

    [Header("Skip Settings")]
    [Tooltip("Can players press space/escape to skip credits?")]
    [SerializeField] private bool allowSkip = true;

    private bool isTransitioning = false;
    private float elapsedScrollTime = 0f;

    void Start()
    {
        allowSkip = false; // Force allowSkip to false so credits are never skippable

        if (creditsTextTransform == null)
        {
            Debug.LogError("[Credits] WARNING: 'creditsTextTransform' is NOT assigned in the Inspector! Please drag your Text object into the slot.");
        }
        else
        {
            RectTransform rect = creditsTextTransform as RectTransform;
            float startY = rect != null ? rect.anchoredPosition.y : creditsTextTransform.position.y;
            Debug.Log($"[Credits] Started! Text current Y: {startY}. Scrolling for {scrollDuration} seconds at {scrollSpeed} speed.");
        }

        // Start with a clean fade-in from black
        if (fadeScreen != null)
        {
            fadeScreen.alpha = 1f;
            StartCoroutine(FadeFromBlack());
        }
        else
        {
            Debug.LogWarning("[Credits] No 'fadeScreen' (CanvasGroup) assigned. Skipping fade-in.");
        }
    }

    void Update()
    {
        if (isTransitioning) return;

        // 1. Move credits upwards smoothly
        if (creditsTextTransform != null)
        {
            RectTransform rect = creditsTextTransform as RectTransform;

            if (rect != null)
            {
                // Modify anchoredPosition directly - 100% robust for UI Canvas items!
                rect.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);
            }
            else
            {
                // Fallback for standard non-UI transforms
                creditsTextTransform.position += new Vector3(0, scrollSpeed * Time.deltaTime, 0);
            }

            // Track elapsed scroll time
            elapsedScrollTime += Time.deltaTime;

            // Check if finished scrolling based on simple duration - 100% immune to coordinate/anchor bugs!
            if (elapsedScrollTime >= scrollDuration)
            {
                Debug.Log($"[Credits] Scroll duration of {scrollDuration}s reached! Starting transition to {nextSceneName}");
                StartCoroutine(EndCreditsTransition());
            }
        }
        else
        {
            // If text is missing, still allow the scene to transition so the player isn't softlocked!
            elapsedScrollTime += Time.deltaTime;
            if (elapsedScrollTime >= 5f) 
            {
                StartCoroutine(EndCreditsTransition());
            }
        }

        // 2. Check for skip input
        if (allowSkip && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0)))
        {
            Debug.Log("[Credits] Skip requested by player input!");
            StartCoroutine(EndCreditsTransition());
        }
    }

    IEnumerator FadeFromBlack()
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

    IEnumerator EndCreditsTransition()
    {
        isTransitioning = true;

        // Fade to black
        float elapsed = 0f;
        if (fadeScreen != null)
        {
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeScreen.alpha = elapsed / fadeDuration;
                yield return null;
            }
            fadeScreen.alpha = 1f;
        }

        // Load Main Menu or next scene
        Debug.Log($"[Credits] Loading next scene: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }
}
