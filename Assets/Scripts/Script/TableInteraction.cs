using UnityEngine;

public class TableInteraction : MonoBehaviour
{
    private Scene9 sceneController;

    void Start()
    {
        // Find the main Scene 9 controller in the scene
        sceneController = Object.FindAnyObjectByType<Scene9>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<KnightHero>() != null)
        {
            if (sceneController != null) sceneController.SetNearTable(true);
            Debug.Log("Player entered table interaction zone.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<KnightHero>() != null)
        {
            if (sceneController != null) sceneController.SetNearTable(false);
            Debug.Log("Player left table interaction zone.");
        }
    }
}
