using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string sceneToLoad; // Name of the next scene (e.g., "Tutorial 1")

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object entering the door has the "Player" tag
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Exiting Level... Loading: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}