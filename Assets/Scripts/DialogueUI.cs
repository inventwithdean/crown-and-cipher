using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    public static Action<NPCProfile, string> OnMessageSubmitted;
    public static Action<NPCProfile> OnConversationEnded;


    public GameObject interactPromptText;
    public GameObject inputUI;
    public TMP_InputField inputField;
    public TextMeshProUGUI npcDialogueText;

    private NPCProfile currentNPC;
    private Action onDialogueClose;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        interactPromptText.SetActive(false);
        inputUI.SetActive(false);
    }

    public void ToggleInteractPrompt(bool show)
    {
        if (!inputUI.activeSelf)
        {
            interactPromptText.SetActive(show);
        }
    }

    public void OpenDialogueBox(NPCProfile npc, Action onCloseCallback)
    {
        currentNPC = npc;
        onDialogueClose = onCloseCallback;

        inputUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        inputField.text = "";

        if (npcDialogueText != null)
        {
            npcDialogueText.text = $"Speaking with {npc.npcName}";
        }

        inputField.ActivateInputField();
    }

    void Update()
    {
        if (inputUI.activeSelf)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                SubmitText();
                inputField.ActivateInputField();
            }
            else if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                OnConversationEnded?.Invoke(currentNPC);
                CloseDialogueBox();
            }
        }
    }

    private void SubmitText()
    {
        if (!string.IsNullOrWhiteSpace(inputField.text) && currentNPC != null)
        {
            OnMessageSubmitted?.Invoke(currentNPC, inputField.text);
            inputField.text = "";
            if (npcDialogueText != null)
            {
                npcDialogueText.text = $"<i>{currentNPC.npcName} is thinking...</i>";
            }
        }

    }

    public void ReceiveNPCDialogue(string npcId, string text)
    {
        if (currentNPC != null && currentNPC.npcID == npcId && npcDialogueText != null)
        {
            npcDialogueText.text = text;
        }
    }

    private void CloseDialogueBox()
    {
        inputUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        onDialogueClose?.Invoke();
        currentNPC = null;
        if (npcDialogueText)
        {
            npcDialogueText.text = "";
        }
    }
}