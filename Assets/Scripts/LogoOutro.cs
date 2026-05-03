using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LogoOutro : MonoBehaviour
{
    public CanvasGroup logoGroup; // Drag the Black Image here
    public float displayTime = 5.0f;
    public float fadeTime = 2.0f;

    void Start()
    {
        StartCoroutine(FadeSequence());
    }

    IEnumerator FadeSequence()
    {
        // 1. Show logo
        logoGroup.alpha = 1f;
        yield return new WaitForSeconds(displayTime);

        // 2. Fade it away
        float counter = 0;
        while (counter < fadeTime)
        {
            counter += Time.deltaTime;
            logoGroup.alpha = 1 - (counter / fadeTime);
            yield return null;
        }

        // 3. Kill it
        gameObject.SetActive(false);
    }
}