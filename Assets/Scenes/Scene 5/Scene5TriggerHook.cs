using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene5TriggerHook : MonoBehaviour
{
    public enum TriggerType { Tablet, Door }
    public TriggerType type;
    
    [Header("Optional Teleport")]
    public string targetSceneName;
    
    private Scene5SequenceController controller;

    void Start()
    {
        controller = FindAnyObjectByType<Scene5SequenceController>();
    }

    // --- EXTRA SENSITIVE: Log everything that touches us ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Log EVERYTHING to the console so we can see if physics is working
        Debug.Log($"<color=white>Trigger Hook: Something touched me! Name: {collision.name}, Tag: {collision.tag}</color>");

        if (collision.CompareTag("Player") || collision.GetComponent<KnightHero>() != null || collision.name.Contains("Knight"))
        {
            Debug.Log($"<color=yellow>Trigger Hook: {type} ACTIVATED by Player!</color>");
            if (controller == null) return;

            if (type == TriggerType.Tablet) controller.SetNearTablet(true);
            if (type == TriggerType.Door) controller.SetNearDoor(true);

            // --- Custom Teleport Logic ---
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                Debug.Log($"<color=green>Trigger Hook: Teleporting to {targetSceneName}!</color>");
                SceneManager.LoadScene(targetSceneName);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<KnightHero>() != null || collision.name.Contains("Knight"))
        {
            Debug.Log($"<color=orange>Trigger Hook: {type} EXITED by Player.</color>");
            if (controller == null) return;

            if (type == TriggerType.Tablet) controller.SetNearTablet(false);
            if (type == TriggerType.Door) controller.SetNearDoor(false);
        }
    }
}
