using UnityEngine;

public class SingleAudioListener : MonoBehaviour
{
    void Awake()
    {
        // Find all AudioListeners in the scene
        AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include);
        
        if (listeners.Length > 1)
        {
            bool keptOne = false;

            // First pass: look for a MainCamera to keep
            foreach (var listener in listeners)
            {
                if (listener.gameObject.CompareTag("MainCamera"))
                {
                    keptOne = true;
                    // Keep this one enabled
                    listener.enabled = true;
                }
                else
                {
                    // Disable it for now, we might enable one later if no MainCamera exists
                    listener.enabled = false;
                }
            }

            // Second pass: if no MainCamera listener was found, keep the first one we saw
            if (!keptOne && listeners.Length > 0)
            {
                listeners[0].enabled = true;
            }

            Debug.Log("Cleaned up AudioListeners. Ensured exactly one is active.");
        }
    }
}
