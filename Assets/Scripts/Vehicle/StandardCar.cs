using UnityEngine;
using UnityEngine.InputSystem;

public class StandardCar : MonoBehaviour
{
    public float motorForce = 1500f;
    public float maxSteerAngle = 35f;

    [Header("Wheel Colliders")]
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider rearLeft;
    public WheelCollider rearRight;

    [Header("Wheel Meshes")]
    public Transform frontLeftMesh;
    public Transform frontRightMesh;
    public Transform rearLeftMesh;
    public Transform rearRightMesh;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
    }

    void FixedUpdate()
    {
        float v = Keyboard.current.wKey.isPressed ? 1 : Keyboard.current.sKey.isPressed ? -1 : 0;
        float h = Keyboard.current.dKey.isPressed ? 1 : Keyboard.current.aKey.isPressed ? -1 : 0;

        // Steering
        float currentSteerAngle = h * maxSteerAngle;
        frontLeft.steerAngle = currentSteerAngle;
        frontRight.steerAngle = currentSteerAngle;

        // Acceleration
        float currentTorque = v * motorForce;
        rearLeft.motorTorque = currentTorque;
        rearRight.motorTorque = currentTorque;
        frontLeft.motorTorque = currentTorque;
        frontRight.motorTorque = currentTorque;

        // Sync visual meshes with physics colliders
        UpdateWheelVisuals(frontLeft, frontLeftMesh);
        UpdateWheelVisuals(frontRight, frontRightMesh);
        UpdateWheelVisuals(rearLeft, rearLeftMesh);
        UpdateWheelVisuals(rearRight, rearRightMesh);
    }

    void UpdateWheelVisuals(WheelCollider collider, Transform mesh)
    {
        collider.GetWorldPose(out Vector3 position, out Quaternion rotation);
        mesh.position = position;
        mesh.rotation = rotation;
    }
}