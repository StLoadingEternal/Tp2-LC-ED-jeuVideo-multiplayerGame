using UnityEngine;

public class PickupController : MonoBehaviour
{
    public const string HORIZONTAL_AXIS = "Horizontal";
    public const string VERTICAL_AXIS = "Vertical";

    [SerializeField]
    private float horizontalInput;

    [SerializeField]
    private float verticalInput;


    private bool isBraking;
    private float currentBrakeForce;
    private float currentSteerAngle;


    [SerializeField]
    private WheelCollider FLWheelCollider;
    [SerializeField]
    private WheelCollider FRWheelCollider;
    [SerializeField]
    private WheelCollider RLWheelCollider;
    [SerializeField]
    private WheelCollider RRWheelCollider;

    [SerializeField]
    private Transform FLWheelTransform;
    [SerializeField]
    private Transform FRWheelTransform;
    [SerializeField]
    private Transform RLWheelTransform;
    [SerializeField]
    private Transform RRWheelTransform;

    [SerializeField]
    private float motorForce;
    [SerializeField]
    private float brakeForce;
    [SerializeField]
    private float maxSteeringAngle;
    [SerializeField]
    private float centreOfGravityOffset = -1f;

    Rigidbody rigidBody;

    // Start is called before the first frame update
    void Start()
    {

        rigidBody = GetComponent<Rigidbody>();

        // Adjust center of mass vertically, to help prevent the car from rolling
        rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;

    }


    void FixedUpdate()
    {
        GetInput();
        HandleMove();
        HandleSteering();
        UpdateWheels();
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis(HORIZONTAL_AXIS);
        verticalInput = Input.GetAxis(VERTICAL_AXIS);
        isBraking = Input.GetKey(KeyCode.Space);
        ApplyBraking();
    }
    private void ApplyBraking()
    {
        FLWheelCollider.brakeTorque = currentBrakeForce;
        FRWheelCollider.brakeTorque = currentBrakeForce;
        RLWheelCollider.brakeTorque = currentBrakeForce;
        RRWheelCollider.brakeTorque = currentBrakeForce;
    }
    private void HandleMove()
    {
        float motorTorque = verticalInput * motorForce;
        FLWheelCollider.motorTorque = motorTorque;
        FRWheelCollider.motorTorque = motorTorque;
        currentBrakeForce = isBraking ? brakeForce : 0f;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteeringAngle * horizontalInput;
        FLWheelCollider.steerAngle = currentSteerAngle;
        FRWheelCollider.steerAngle = currentSteerAngle;
    }
    private void UpdateWheels()
    {
        UpdateWheel(FLWheelCollider, FLWheelTransform);
        UpdateWheel(FRWheelCollider, FRWheelTransform);
        UpdateWheel(RLWheelCollider, RLWheelTransform);
        UpdateWheel(RRWheelCollider, RRWheelTransform);
    }

    private void UpdateWheel(WheelCollider wheelCollider, Transform tranform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        tranform.rotation = rot;
        tranform.position = pos;
    }
}