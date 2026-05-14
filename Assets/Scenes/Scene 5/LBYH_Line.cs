using UnityEngine;

// Renamed to LBYH_Line to prevent ANY global namespace conflicts
[System.Serializable]
public class LBYH_Line
{
    public string name;
    [TextArea(3, 10)]
    public string text;
    public AudioClip voiceClip;
}
