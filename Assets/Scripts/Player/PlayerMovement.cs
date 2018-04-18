using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class PlayerMovement : MonoBehaviour 
{
    [SerializeField] Rigidbody rb;
    public float idealRPM = 500f;
    public float maxRPM = 1000f;

    public Transform centerOfGravity;

    public WheelCollider wheelFR;
    public WheelCollider wheelFL;
    public WheelCollider wheelRR;
    public WheelCollider wheelRL;

    public float turnRadius = 6f;
    public float torque = 25f;
    public float brakeTorque = 100f;

    public float antiRoll = 20000;

    public enum DriveMode
    {
        FRONT, REAR, ALL
    };
    public DriveMode driveMode = DriveMode.REAR;

    // Insert Speed Text DEBUG

	PlayerIndex playerCont;
	GamePadState controllerState;

	void Awake()
	{
		playerCont = PlayerIndex.One;
        rb.centerOfMass = centerOfGravity.localPosition;
	}

    public float Speed()
    {
        return wheelRR.radius * Mathf.PI * wheelRR.rpm * 60f / 1000f;
    }

    public float Rpm()
    {
        return wheelRL.rpm;
    }

	void FixedUpdate () 
    {
		controllerState = GamePad.GetState (playerCont);

		if (controllerState.IsConnected) 
		{
            if (controllerState.Buttons.A == ButtonState.Pressed)
            {
                float scaledTorque = torque;

                if (wheelRL.rpm < idealRPM)
                    scaledTorque = Mathf.Lerp(scaledTorque / 10f, scaledTorque, wheelRL.rpm / idealRPM);
                else
                    scaledTorque = Mathf.Lerp(scaledTorque, 0f, (wheelRL.rpm - idealRPM) / (maxRPM - idealRPM));

                DoRollBar(wheelFR, wheelFL);
                DoRollBar(wheelRR, wheelRL);

                wheelFR.motorTorque = driveMode == DriveMode.REAR ? 0f : scaledTorque;
                wheelFL.motorTorque = driveMode == DriveMode.REAR ? 0f : scaledTorque;
                wheelRR.motorTorque = driveMode == DriveMode.FRONT ? 0f : scaledTorque;
                wheelRL.motorTorque = driveMode == DriveMode.FRONT ? 0f : scaledTorque;
            }

            wheelFR.steerAngle = controllerState.ThumbSticks.Left.X * turnRadius;
            wheelFL.steerAngle = controllerState.ThumbSticks.Left.X * turnRadius;

            if (controllerState.Buttons.B == ButtonState.Pressed)
            {
                wheelFR.brakeTorque = brakeTorque;
                wheelFL.brakeTorque = brakeTorque;
                wheelRR.brakeTorque = brakeTorque;
                wheelRL.brakeTorque = brakeTorque;
            }
            else
            {
                wheelFR.brakeTorque = 0f;
                wheelFL.brakeTorque = 0f;
                wheelRR.brakeTorque = 0f;
                wheelRL.brakeTorque = 0f;
            }
		}
	}

    void DoRollBar(WheelCollider WheelL, WheelCollider wheelR)
    {
        WheelHit hit;
        float travelL = 1f;
        float travelR = 1f;

        bool groundedL = WheelL.GetGroundHit(out hit);
        if (groundedL)
            travelL = (-WheelL.transform.InverseTransformPoint(hit.point).y - WheelL.radius) / WheelL.suspensionDistance;

        bool groundedR = wheelR.GetGroundHit(out hit);
        if (groundedR)
            travelR = (-wheelR.transform.InverseTransformPoint(hit.point).y - wheelR.radius) / wheelR.suspensionDistance;

        float antiRollForce = (travelL - travelR) * antiRoll;

        if (groundedL)
            rb.AddForceAtPosition(WheelL.transform.up * -antiRollForce, WheelL.transform.position);

        if (groundedR)
            rb.AddForceAtPosition(wheelR.transform.up * -antiRollForce, wheelR.transform.position);
    }
}
