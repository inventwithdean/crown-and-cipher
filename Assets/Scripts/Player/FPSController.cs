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

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;

    public InputAction moveAction;
    public InputAction lookAction;
    public InputAction jumpAction;

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
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        HandleLook();
        HandleMovement();
    }

    void HandleLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

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
}
