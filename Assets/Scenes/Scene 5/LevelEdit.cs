using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour {
    public string nextSceneName = "Scene6";
    public bool requiresKeycard = true;
    public static bool hasKeycard = false;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            if (!requiresKeycard || hasKeycard) {
                SceneManager.LoadScene(nextSceneName);
            } else {
                Debug.Log("Locked! Need Keycard from Bully.");
            }
        }
    }
}