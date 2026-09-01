using System.Runtime.InteropServices;
using UnityEngine;

public class WebMCPManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [DllImport("__Internal")]
    private static extern void InitWebMCPTools();

    [DllImport("__Internal")]
    private static extern void EnqueuePlayerMessage(string npcId, string message);

    [DllImport("__Internal")]
    private static extern void EnqueueInteractionEvent(string message);

    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        InitWebMCPTools();
        DialogueUI.OnMessageSubmitted += HandlePlayerMessage;
        DialogueUI.OnConversationEnded += HandleConversationEnded;
        
#endif
    }

    private void OnDestroy()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        DialogueUI.OnMessageSubmitted -= HandlePlayerMessage;
        DialogueUI.OnConversationEnded -= HandleConversationEnded;
#endif

    }

    private void HandlePlayerMessage(NPCProfile npc, string message)
    {
        EnqueuePlayerMessage(npc.npcName, message);
    }

    private void HandleConversationEnded(NPCProfile npc)
    {
        EnqueueInteractionEvent($"Conversation ended with {npc.npcName}");
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

}
