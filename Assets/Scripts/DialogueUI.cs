using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    // WebMCPManager subscribes to this delegate
    public static Action<NPCProfile, string> OnMessageSubmitted;

    [Header("UI Elements")]
    public GameObject interactPromptText;
    public GameObject inputUI;
    public TMP_InputField inputField;
    public TextMeshProUGUI npcDialogueText;

    [Header("Typewriter Effect")]
    public float typingSpeed = 0.03f;
    public AudioClip typingSound;

    private NPCProfile currentNPC;
    private Action onDialogueClose;
    private AudioSource audioSource;
    private Coroutine typingCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        interactPromptText.SetActive(false);
        inputUI.SetActive(false);
        audioSource = GetComponent<AudioSource>();
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

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

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
            // Only input this class handles is the enter key to send the message
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                SubmitText();
                inputField.ActivateInputField();
            }
        }
    }

    private void SubmitText()
    {
        if (!string.IsNullOrWhiteSpace(inputField.text) && currentNPC != null)
        {
            OnMessageSubmitted?.Invoke(currentNPC, inputField.text);
            inputField.text = "";


            if (typingCoroutine != null) StopCoroutine(typingCoroutine);

            if (npcDialogueText != null)
            {
                npcDialogueText.text = $"<i>{currentNPC.npcName} is thinking...</i>";
            }
        }

    }

    public void ReceiveNPCDialogue(string npcName, string text)
    {
        if (currentNPC != null && currentNPC.npcName == npcName && npcDialogueText != null)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(text));
        }
    }

    private IEnumerator TypeText(string textToType)
    {
        npcDialogueText.text = "";
        foreach (char c in textToType)
        {
            npcDialogueText.text += c;
            if (audioSource != null && typingSound != null && char.IsLetterOrDigit(c))
            {
                audioSource.PlayOneShot(typingSound);
            }
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void CloseDialogueBox()
    {
        inputUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        onDialogueClose?.Invoke();
        currentNPC = null;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        if (npcDialogueText)
        {
            npcDialogueText.text = "";
        }
    }
}