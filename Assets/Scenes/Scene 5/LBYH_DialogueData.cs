using UnityEngine;

/// <summary>
/// Asset you can create (Assets → Create → LBYH → Dialogue Line Block) and drag into Scene5SequenceController.
/// Lines are edited inside the asset — not dragged onto the "Dialogue" slot (that slot is for the LBYH_Dialogue UI component).
/// </summary>
[CreateAssetMenu(fileName = "DialogueLines", menuName = "LBYH/Dialogue Line Block", order = 0)]
public class LBYH_DialogueData : ScriptableObject
{
    public LBYH_Line[] lines;
}
