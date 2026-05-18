using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    public Image fadeImage;
    public float defaultFadeDuration = 1.0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // --- FIXED: Start CLEAR so the game is visible ---
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0; 
            fadeImage.color = c;
        }
    }

    // No auto-fade in Start() as requested. 
    // This will only fade when something calls it (like your Scene Changer).

    public IEnumerator FadeIn()
    {
        yield return StartCoroutine(Fade(1, 0));
    }

    public IEnumerator FadeOut()
    {
        yield return StartCoroutine(Fade(0, 1));
    }

    public IEnumerator Fade(float startAlpha, float endAlpha, float duration = -1)
    {
        if (fadeImage == null) yield break;
        if (duration <= 0) duration = defaultFadeDuration;

        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Color c = fadeImage.color;
            c.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            fadeImage.color = c;
            yield return null;
        }

        Color final = fadeImage.color;
        final.a = endAlpha;
        fadeImage.color = final;
    }
}
