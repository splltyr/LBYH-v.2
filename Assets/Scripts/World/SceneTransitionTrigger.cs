using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Scene Settings")]
    public string nextSceneName = "Scene 9"; 
    public float fadeDuration = 1.0f;
    
    [Header("Detection")]
    public LayerMask playerLayer;
    public bool triggerOnTouch = true;

    private bool isTransitioning = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTransitioning) return;

        // Ultra-robust check to detect the player regardless of layer, child-collider nesting, or tag structure!
        bool isPlayer = collision.CompareTag("Player") || 
                        collision.transform.root.CompareTag("Player") || 
                        collision.gameObject.name.ToLower().Contains("player") ||
                        collision.gameObject.name.ToLower().Contains("knight") ||
                        collision.GetComponentInParent<KnightHero>() != null ||
                        collision.GetComponentInChildren<KnightHero>() != null;

        if (isPlayer)
        {
            if (triggerOnTouch)
            {
                StartCoroutine(TransitionToScene());
            }
        }
    }

    IEnumerator TransitionToScene()
    {
        isTransitioning = true;
        Debug.Log($"<color=green>SceneTransition: Fading and loading {nextSceneName}...</color>");

        // Failsafe fade: wait for ScreenFader to finish, but force a maximum wait of 1.5 seconds to prevent soft-locks!
        bool fadeFinished = false;
        IEnumerator RunFade()
        {
            if (ScreenFader.Instance != null)
            {
                yield return StartCoroutine(ScreenFader.Instance.FadeOut());
            }
            fadeFinished = true;
        }

        StartCoroutine(RunFade());
        float timer = 0f;
        while (!fadeFinished && timer < 1.5f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null) Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
    }
}
