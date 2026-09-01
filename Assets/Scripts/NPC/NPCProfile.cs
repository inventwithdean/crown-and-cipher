using UnityEngine;

public class NPCProfile: MonoBehaviour
{
    public string npcName = "NPCName";
    [TextArea(3, 10)]
    public string description = "NPC's description";
}