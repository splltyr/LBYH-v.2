using UnityEngine;
using System.Collections;

public class SpawnCutscene : MonoBehaviour
{
    [Header("Settings")]
    public float freezeDuration = 2.0f;     // Match this to your portal animation length
    public string portalStateName = "Portal"; // The name of the box in Animator

    private MonoBehaviour movementScript;
    private Animator anim;

    void Start()
    {
        // 1. Find the Player in the Tutorial scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            // Now supports both KnightHero and legacy PlayerMovement
            movementScript = player.GetComponent<KnightHero>();
            if (movementScript == null) movementScript = player.GetComponent<MonoBehaviour>(); // Fallback
            
            anim = player.GetComponent<Animator>();

            if (movementScript != null && anim != null)
            {
                StartCoroutine(ExecutePortalCutscene(player));
            }
        }
    }

    IEnumerator ExecutePortalCutscene(GameObject player)
    {
        // 2. Kill the movement script so 'Update' doesn't run
        if (movementScript != null) movementScript.enabled = false;
        
        // Zero out physical velocity so they don't slide!
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 3. CLEAN RESET: Force 'isWalking' to false immediately
        if (anim != null) 
        {
            anim.SetBool("isWalking", false); 
            anim.Play("KnightIdle"); // Force idle animation
        }

        Debug.Log("Spawn Cutscene Started: Player Frozen.");

        // 5. Wait for the portal to finish
        yield return new WaitForSeconds(freezeDuration);

        // 6. Give control back
        movementScript.enabled = true;
        Debug.Log("Spawn Cutscene Finished: Player Free.");
    }
}