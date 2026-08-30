using System.Runtime.InteropServices;
using UnityEngine;

public class WebMCPManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [DllImport("__Internal")]
    private static extern void InitWebMCPTools();

    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        InitWebMCPTools();
#endif
    }

    [System.Serializable]
    public class DialogueData
    {
        public int id;
        public string dialogue;
    }

    public void ReceiveDialogue(string jsonPayload)
    {
        DialogueData parsedData = JsonUtility.FromJson<DialogueData>(jsonPayload);
        Debug.Log($"NPC {parsedData.id} Speaking: {parsedData.dialogue}");
    }

    public void SpawnItem(string itemId, string locationX, string locationZ)
    {
        Debug.Log($"Spawning {itemId} at X:{locationX} Z:{locationZ}");
    }

}
