using Unity.Netcode;
using UnityEngine;

public class RacingCarControl : NetworkBehaviour
{
    public float motorTorque = 2000;
    public float brakeTorque = 2000;
    public float maxSpeed = 20;
    public float steeringRange = 30;
    public float steeringRangeAtMaxSpeed = 10;
    public float centreOfGravityOffset = -1f;

    WheelControl[] wheels;
    Rigidbody rigidBody;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;
        wheels = GetComponentsInChildren<WheelControl>();
    }

    void Update()
    {
        // Seulement le owner envoie ses inputs
        if (!IsOwner) return;

        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");

        // Envoie les inputs au serveur
        MoveServerRpc(vInput, hInput);
    }

    [ServerRpc]
    void MoveServerRpc(float vInput, float hInput)
    {
        UpdateCarMovement(vInput, hInput);
    }

    private void UpdateCarMovement(float vInput, float hInput)
    {
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);
        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);
        bool isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);

        foreach (var wheel in wheels)
        {
            if (wheel.steerable)
                wheel.WheelCollider.steerAngle = hInput * currentSteerRange;

            if (isAccelerating)
            {
                if (wheel.motorized)
                    wheel.WheelCollider.motorTorque = vInput * currentMotorTorque;
                wheel.WheelCollider.brakeTorque = 0;
            }
            else
            {
                wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
                wheel.WheelCollider.motorTorque = 0;
            }
        }
    }
}