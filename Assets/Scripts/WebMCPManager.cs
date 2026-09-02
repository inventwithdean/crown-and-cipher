using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class WebMCPManager : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void InitWebMCPTools();

    [DllImport("__Internal")]
    private static extern void EnqueuePlayerMessage(string npcId, string message);

    [DllImport("__Internal")]
    private static extern void EnqueueInteractionEvent(string message);

    [DllImport("__Internal")]
    private static extern void ReturnSystemContext(string context);

    // [TextArea(3, 10)]
    private string systemPrompt = "This is a WebMCP powered Unity WebGL game, setup in a city. There has been a murder of a guy and her wife is standing near him and continuously crying. You are given some story data to weave the story as player interacts with these NPCs. When the player interacts with them, you'll possess the NPC and use the speak tool to speak to the player from that NPC's viewpoint. Craft a beautiful story as the game goes on. The player has an orange sedan which they'll use to travel the city. Use only the specified locations, and to get any details about an NPC, just use the get_npc_location tool. So use that to craft a rich quest experience, where the player would travel and meet people and go back and forth collecting information and asking right questions, finally catching the killer. Use the wait tool, and keep using the wait tool in loop until the player tells you to stop. Until then, do NOT exit the game loop, which is the wait tool. You can use any tool you like, but must call the wait tool instantly after every tool, and keep on going.";

    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        InitWebMCPTools();
        DialogueUI.OnMessageSubmitted += HandlePlayerMessage;
#endif
    }

    private void OnDestroy()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        DialogueUI.OnMessageSubmitted -= HandlePlayerMessage;
#endif

    }

    // When player sends a message to NPC, this function receives it. Sending it to WebMCPBridge.jslib plugin
    private void HandlePlayerMessage(NPCProfile npc, string message)
    {
        EnqueuePlayerMessage(npc.npcName, message);
    }


    [System.Serializable]
    public class DialogueData
    {
        public string npcName;
        public string dialogue;
    }

    public void ReceiveDialogue(string jsonPayload)
    {
        DialogueData parsedData = JsonUtility.FromJson<DialogueData>(jsonPayload);

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ReceiveNPCDialogue(parsedData.npcName, parsedData.dialogue);
        }
    }

    // Decide who the killer is and setup the chain of key NPCs 
    public void GetSystemContext()
    {
        string rosterContext = NPCManager.Instance.GetRosterContext();
        string locationsContext = LocationMarkers.Instance.GetLocationsContext();

        // Clone the list
        List<NPCProfile> availableNPCs = new List<NPCProfile>(NPCManager.Instance.allNPCs);

        for (int i = 0; i < availableNPCs.Count; i++)
        {
            NPCProfile temp = availableNPCs[i];
            int randomIndex = Random.Range(i, availableNPCs.Count);
            availableNPCs[i] = availableNPCs[randomIndex];
            availableNPCs[randomIndex] = temp;
        }

        string storyConstraints = "";
        string knowledgeConstraints = "";

        // This will always be true, but just to be safe
        if (availableNPCs.Count >= 7)
        {
            string killer = availableNPCs[0].npcName;
            string key1 = availableNPCs[1].npcName;
            string key2 = availableNPCs[2].npcName;
            string key3 = availableNPCs[3].npcName;

            string bridge1 = availableNPCs[4].npcName;
            string bridge2 = availableNPCs[5].npcName;
            string bridge3 = availableNPCs[6].npcName;

            storyConstraints = $"STORY DATA:\n" +
                               $"The Killer is: {killer}\n" +
                               $"The player MUST interact with these 3 key NPCs before catching the killer: {key1}, {key2}, and {key3}.\n" +
                               $"Do not allow the player to successfuly identify or arrest the killer until they have gathered clues from all three of these key NPCs.";

            Dictionary<string, List<string>> npcKnowledge = new Dictionary<string, List<string>>();
            foreach (var npc in availableNPCs)
            {
                npcKnowledge[npc.npcName] = new List<string>();
            }

            // Establish the indirect breadcrumb chain via random bridges:
            npcKnowledge[key1].Add(bridge1);
            npcKnowledge[bridge1].Add(key2);

            npcKnowledge[key2].Add(bridge2);
            npcKnowledge[bridge2].Add(key3);

            npcKnowledge[key3].Add(bridge3);
            npcKnowledge[bridge3].Add(killer);

            // Fill the rest of everyone's knowledge slots up to a max of 3 random connections
            foreach (var npc in availableNPCs)
            {
                while (npcKnowledge[npc.npcName].Count < 3)
                {
                    string randomName = availableNPCs[Random.Range(0, availableNPCs.Count)].npcName;
                    if (randomName != npc.npcName && !npcKnowledge[npc.npcName].Contains(randomName))
                    {
                        npcKnowledge[npc.npcName].Add(randomName);
                    }
                }
            }

            knowledgeConstraints = "NPC Knowledge Limitations:\n" +
                                  "NPCs ONLY know the specific people listed below. If the player asks an NPC about someone NOT in their known list, the NPC must deny knowing them or claim ignorance.\n";
            foreach (var kvp in npcKnowledge)
            {
                knowledgeConstraints += $"{kvp.Key} knows: {string.Join(", ", kvp.Value)}\n";
            }
        }

        else
        {
            Debug.LogWarning("Not enough NPCs in the scene to assign a killer and 3 key NPCs or to setup indirect knowledge graph. Make sure the scene has atleast 7 NPCs.");
        }

        ReturnSystemContext($"{systemPrompt}\n\n{storyConstraints}\n\n{knowledgeConstraints}\n\n{rosterContext}\n\n{locationsContext}");
    }

}
