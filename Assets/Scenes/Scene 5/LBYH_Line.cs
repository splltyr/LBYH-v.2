using UnityEngine;

// Renamed to LBYH_Line to prevent ANY global namespace conflicts
[System.Serializable]
public class LBYH_Line
{
    public string name;
    [TextArea(3, 10)]
    public string text;
    public AudioClip voiceClip;

    [Tooltip("Control the volume! 1 is normal, 5 is 5x Louder!")]
    [Range(0f, 20f)]
    public float volume = 1f;
}
