using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public enum TutorialType { Jump, Slide, Boss, Custom }
    public TutorialType type;
    public string customMessage;
    
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            Scene1Controller controller = Object.FindAnyObjectByType<Scene1Controller>();
            // ONLY trigger if the intro sequence is finished!
            if (controller != null && (controller.IsIntroDone() || type == TutorialType.Boss))
            {
                hasTriggered = true;
                if (type == TutorialType.Jump) controller.TriggerJumpTutorial();
                else if (type == TutorialType.Slide) controller.TriggerSlideTutorial();
                else if (type == TutorialType.Boss) controller.TriggerBossDialogue();
                else controller.ShowTutorial(customMessage);
            }
        }
    }
}
