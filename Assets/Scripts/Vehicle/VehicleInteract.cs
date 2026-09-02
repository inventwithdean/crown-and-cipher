using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleInteract : MonoBehaviour
{
    public GameObject vehicleCamera;
    public MonoBehaviour carController;
    public Transform exitPoint;

    private GameObject player;
    private bool isPlayerNear = false;
    private bool isDriving = false;


    void Awake()
    {
        vehicleCamera.SetActive(false);
        carController.enabled = false;

    }
    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (isPlayerNear && !isDriving) EnterVehicle();
            else if (isDriving) ExitVehicle();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            player = other.gameObject;
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

    private void EnterVehicle()
    {
        isDriving = true;
        DialogueUI.Instance.ToggleInteractPrompt(false);
        player.SetActive(false);
        vehicleCamera.SetActive(true);
        carController.enabled = true;
    }

    private void ExitVehicle()
    {
        isDriving = false;
        player.transform.position = exitPoint.position;
        player.SetActive(true);
        vehicleCamera.SetActive(false);
        carController.enabled = false;
    }
}