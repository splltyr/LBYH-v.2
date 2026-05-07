using UnityEngine;

public class Scene3Manager : MonoBehaviour
{
    public enum SceneState { IntroChat, SkeletonAmbush, RescueReya, HuntCooker, Reward }
    public SceneState currentState = SceneState.IntroChat;

    [Header("Scene Objects")]
    public GameObject skeletonGroup; 
    public GameObject corruptedCooker;

    void Start()
    {
        // Keep the boss and skeletons hidden at the very start
        if(skeletonGroup != null) skeletonGroup.SetActive(false);
        if(corruptedCooker != null) corruptedCooker.SetActive(false);
    }

    public void FinishIntroChat()
    {
        currentState = SceneState.SkeletonAmbush;
        if(skeletonGroup != null) skeletonGroup.SetActive(true);
    }

    public void EnemyDefeated(string enemyType)
    {
        if (enemyType == "Skeleton" && currentState == SceneState.SkeletonAmbush)
        {
            currentState = SceneState.RescueReya;
            Debug.Log("Skeletons cleared! Go talk to Reya.");
        }
        else if (enemyType == "CorruptedCooker")
        {
            currentState = SceneState.Reward;
            Debug.Log("Boss defeated! Get the keycard from Reya.");
        }
    }
}