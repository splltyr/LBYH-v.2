using UnityEngine; // This is the line that was missing!

public class ClinicExit : MonoBehaviour
{
    public ClinicDialogueManager manager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object hitting the door has the "Player" tag
        if (other.CompareTag("Player"))
        {
            manager.StartFinalExit();
        }
    }
}