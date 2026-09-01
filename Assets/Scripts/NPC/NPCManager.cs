using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }
    public List<NPCProfile> allNPCs;

    [DllImport("__Internal")]
    private static extern void ReturnNPCLocation(string locationData);

    [System.Serializable]
    public class LocationRequest
    {
        public string npcName;
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Auto collect all NPCs in the scene at startup
        allNPCs = new List<NPCProfile>(FindObjectsByType<NPCProfile>());
    }

    // Returns a compiled string of all NPCs for LLM to digest
    public string GetRosterContext()
    {
        string context = "City NPC Roster:\n";
        foreach (var npc in allNPCs)
        {
            context += $"- {npc.npcName}: {npc.description}\n";
        }
        return context;
    }
    public void GetNPCLocationContext(string jsonPayload)
    {
        LocationRequest req = JsonUtility.FromJson<LocationRequest>(jsonPayload);
        NPCProfile targetNPC = allNPCs.Find(n => n.npcName == req.npcName);

        string result = "NPC not found.";

        if (targetNPC != null)
        {
            result = LocationMarkers.GetTopTwoClosestLocations(targetNPC.transform);
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        ReturnNPCLocation(result);
#endif
    }

}