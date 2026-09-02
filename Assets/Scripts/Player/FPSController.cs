using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public float walkSpeed = 6f;
    public float lookSensitivity = 0.15f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;
    public Transform cameraTarget;

    public float interactDistance = 3f;
    public NPCInteractable currentTarget;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;

    public InputAction moveAction;
    public InputAction lookAction;
    public InputAction jumpAction;

    private bool controllable = false;
    public static FPSController Instance = null;

    void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!controllable) return;
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (DialogueUI.Instance != null && !DialogueUI.Instance.inputUI.activeSelf)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        HandleLook();
        HandleMovement();
        HandleInteraction();
    }


    void HandleLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked || Cursor.visible == true) return;

        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTarget.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector2 moveInput = Vector2.zero;
        bool jumpPressed = false;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            moveInput = moveAction.ReadValue<Vector2>();
            jumpPressed = jumpAction.triggered;
        }


        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * walkSpeed * Time.deltaTime);

        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleInteraction()
    {
        if (DialogueUI.Instance != null && DialogueUI.Instance.inputUI.activeSelf)
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                DialogueUI.Instance.CloseDialogueBox();
                // We just closed the box, so we probably are looking at the NPC, so we should show the interact prompt
                if (currentTarget) DialogueUI.Instance.ToggleInteractPrompt(true);
            }
            return;
        }

        Ray ray = new Ray(cameraTarget.position, cameraTarget.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            NPCInteractable interactable = hit.collider.GetComponent<NPCInteractable>();
            if (interactable != null)
            {
                if (currentTarget != interactable)
                {
                    currentTarget = interactable;
                    DialogueUI.Instance.ToggleInteractPrompt(true);
                }

                if (Keyboard.current.tabKey.wasPressedThisFrame)
                {
                    currentTarget.Interact(transform);
                }
                return; // Exit early if we are looking at an NPC
            }
        }

        if (currentTarget != null)
        {
            currentTarget = null;
            DialogueUI.Instance.ToggleInteractPrompt(false);
        }
    }

    public void setControllable(bool isControllable)
    {
        controllable = isControllable;
    }
}
