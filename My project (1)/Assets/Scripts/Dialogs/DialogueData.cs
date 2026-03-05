using UnityEngine;

[CreateAssetMenu(fileName = "Yeni Robot Diyalog", menuName = "Diyalog Sistemi/Veri")]
public class DialogueData : ScriptableObject
{
    public string robotIsmi; 
    [TextArea(3, 10)]
    public string[] cumleler; // Buraya istediğin kadar satır ekleyebilirsin
}