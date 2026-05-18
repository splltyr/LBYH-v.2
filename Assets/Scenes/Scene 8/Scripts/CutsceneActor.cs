using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class CutsceneActor : MonoBehaviour
{
    [Header("Manual Animation")]
    [Tooltip("Drag your Idle Animation Clip here! No Animator Controller needed!")]
    public AnimationClip idleClip;
    
    private PlayableGraph graph;

    void OnEnable()
    {
        if (idleClip == null) return;
        
        graph = PlayableGraph.Create("ActorGraph_" + gameObject.name);
        
        // Auto-add Animator component if missing
        var animator = GetComponentInChildren<Animator>();
        if (animator == null) animator = gameObject.AddComponent<Animator>();
        
        AnimationPlayableOutput.Create(graph, "Animation", animator);
        
        var clipPlayable = AnimationClipPlayable.Create(graph, idleClip);
        graph.GetOutput(0).SetSourcePlayable(clipPlayable);
        
        graph.Play();
    }

    void OnDisable()
    {
        if (graph.IsValid()) graph.Destroy();
    }

    void OnDestroy()
    {
        if (graph.IsValid()) graph.Destroy();
    }
}
