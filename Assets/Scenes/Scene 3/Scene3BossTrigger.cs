using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Scene3BossTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        // Ensure the collider is set to "Is Trigger"
        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            
            if (Scene3Manager.Instance != null)
            {
                Scene3Manager.Instance.TriggerCookerIntro();
            }
        }
    }
}
