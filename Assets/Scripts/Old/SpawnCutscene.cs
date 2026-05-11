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
            // Change 'PlayerMovement' to match your movement script's name exactly
            movementScript = player.GetComponent<PlayerMovement>(); 
            anim = player.GetComponent<Animator>();

            if (movementScript != null && anim != null)
            {
                StartCoroutine(ExecutePortalCutscene());
            }
        }
    }

    IEnumerator ExecutePortalCutscene()
    {
        // 2. Kill the movement script so 'Update' doesn't run
        movementScript.enabled = false;

        // 3. CLEAN RESET: Force 'isWalking' to false immediately
        // This prevents the player from "remembering" they were walking in the last scene
        anim.SetBool("isWalking", false); 
        
        // If your script uses a float called 'Speed' or similar, reset it here too:
        // anim.SetFloat("Speed", 0f);

        // 4. FORCE the Animator to play the Portal state
        anim.Play(portalStateName); 

        Debug.Log("Spawn Cutscene Started: Player Frozen.");

        // 5. Wait for the portal to finish
        yield return new WaitForSeconds(freezeDuration);

        // 6. Give control back
        movementScript.enabled = true;
        Debug.Log("Spawn Cutscene Finished: Player Free.");
    }
}