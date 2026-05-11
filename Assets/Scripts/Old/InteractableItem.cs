using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [SerializeField] private float interactionRange = 2f; // How close the player needs to be
    private Transform player;

    void Start()
    {
        // Find the player automatically using the Tag we set up earlier
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        // Check distance between this item and the player
        float distance = Vector2.Distance(transform.position, player.position);

        // If player is close AND presses E
        if (distance <= interactionRange && Input.GetKeyDown(KeyCode.E))
        {
            Pickup();
        }
    }

    private void Pickup()
    {
        Debug.Log("Item picked up!");
        
        // This makes the object disappear from the game
        Destroy(gameObject);
    }

    // Visual helper to see the interaction range in the Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}