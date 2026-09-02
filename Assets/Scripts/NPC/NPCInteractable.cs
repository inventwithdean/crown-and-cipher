using System.Collections;
using UnityEngine;

[RequireComponent(typeof(NPCProfile))]
public class NPCInteractable : MonoBehaviour
{
    private NPCProfile profile;
    private NPCWander wanderScript;
    private bool isTalking = false;
    private Transform playerTransform;

    void Start()
    {
        profile = GetComponent<NPCProfile>();
        wanderScript = GetComponent<NPCWander>();
    }

    public void Interact(Transform player)
    {
        if (isTalking) return;
        playerTransform = player;
        isTalking = true;
        DialogueUI.Instance.ToggleInteractPrompt(false);
        wanderScript.SetWanderState(false);

        StartCoroutine(FacePlayer());
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