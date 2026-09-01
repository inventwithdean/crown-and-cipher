using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NPCProfile))]
public class NPCInteractable : MonoBehaviour
{
    private NPCProfile profile;
    private NPCWander wanderScript;
    private bool isPlayerNear = false;
    private bool isTalking = false;
    private Transform playerTransform;

    void Start()
    {
        profile = GetComponent<NPCProfile>();
        wanderScript = GetComponent<NPCWander>();
    }

    void Update()
    {
        if (isPlayerNear && !isTalking && Keyboard.current.fKey.wasPressedThisFrame)
        {
            StartConversation();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerTransform = other.transform;
            DialogueUI.Instance.ToggleInteractPrompt(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            DialogueUI.Instance.ToggleInteractPrompt(false);
        }
    }

    private void StartConversation()
    {
        isTalking = true;
        DialogueUI.Instance.ToggleInteractPrompt(false);
        wanderScript.SetWanderState(false);

        StartCoroutine(FacePlayer());

        // Pass this NPC's profile and a callback to unpause them when UI closes.
        DialogueUI.Instance.OpenDialogueBox(profile, EndConversation);
    }

    private void EndConversation()
    {
        isTalking = false;
        wanderScript.SetWanderState(true);
    }

    private IEnumerator FacePlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        while (isTalking)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            yield return null;
        }
    }
}