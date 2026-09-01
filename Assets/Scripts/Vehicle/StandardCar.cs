using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class StandardCar : MonoBehaviour
{
    public float motorForce = 1500f;
    public float brakeForce = 3000f;
    public float decelerationForce = 300f;
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

    [Header("Audio Settings")]
    public float minPitch = 0.8f;
    public float maxPitch = 2.5f;
    public float maxSpeedForAudio = 30f;

    private Rigidbody rb;
    private AudioSource carAudio;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
        carAudio = GetComponent<AudioSource>();
        carAudio.loop = true;
        if (!carAudio.isPlaying)
        {
            carAudio.Play();
        }
    }

    void OnEnable()
    {
        if (carAudio != null && !carAudio.isPlaying)
        {
            carAudio.Play();
        }

        ApplyBrakes(0f);
    }

    void OnDisable()
    {
        if (carAudio != null && carAudio.isPlaying)
        {
            carAudio.Stop();
        }

        ResetVehicleInputs();
    }

    void FixedUpdate()
    {
        float v = Keyboard.current.wKey.isPressed ? 1 : Keyboard.current.sKey.isPressed ? -1 : 0;
        float h = Keyboard.current.dKey.isPressed ? 1 : Keyboard.current.aKey.isPressed ? -1 : 0;
        bool isBraking = Keyboard.current.spaceKey.isPressed;

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

        // Braking
        float currentBrakeTorque = 0f;
        if (isBraking)
        {
            currentBrakeTorque = brakeForce;
        }
        else if (v == 0)
        {
            currentBrakeTorque = decelerationForce;
        }
        ApplyBrakes(currentBrakeTorque);

        // Sync visual meshes with physics colliders
        UpdateWheelVisuals(frontLeft, frontLeftMesh);
        UpdateWheelVisuals(frontRight, frontRightMesh);
        UpdateWheelVisuals(rearLeft, rearLeftMesh);
        UpdateWheelVisuals(rearRight, rearRightMesh);
    }

    void Update()
    {
        UpdateEngineAudio();
    }

    void UpdateWheelVisuals(WheelCollider collider, Transform mesh)
    {
        collider.GetWorldPose(out Vector3 position, out Quaternion rotation);
        mesh.position = position;
        mesh.rotation = rotation;
    }

    void UpdateEngineAudio()
    {
        float currentSpeed = rb.linearVelocity.magnitude;
        carAudio.pitch = Mathf.Lerp(minPitch, maxPitch, currentSpeed / maxSpeedForAudio);
    }
    void ApplyBrakes(float force)
    {
        frontLeft.brakeTorque = force;
        frontRight.brakeTorque = force;
        rearLeft.brakeTorque = force;
        rearRight.brakeTorque = force;
    }

    void ResetVehicleInputs()
    {
        frontLeft.steerAngle = 0;
        frontRight.steerAngle = 0;

        frontLeft.motorTorque = 0;
        frontRight.motorTorque = 0;
        rearLeft.motorTorque = 0;
        rearRight.motorTorque = 0;

        ApplyBrakes(brakeForce);
    }
}