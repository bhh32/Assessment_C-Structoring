using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class PlayerMovement : MonoBehaviour 
{
    PlayerIndex player;
    GamePadState contState;

    [SerializeField] Rigidbody rb;
    public GameObject wheel_FR;
    public GameObject wheel_FL;
    public GameObject wheel_RR;
    public GameObject wheel_RL;

    public WheelCollider W_FL;
    public WheelCollider W_FR;
    public WheelCollider W_RR;
    public WheelCollider W_RL;

    public float torque = 1000f;
    public float brakingTorque = 1000f;
    public float lowestSteeringSpeed = 20f;
    public float lowestSteeringAngle = 70f;
    public float highestSteeringAngle = 40f;

    private void Awake()
    {
        player = PlayerIndex.One;
    }

    private void FixedUpdate()
    {
        contState = GamePad.GetState(player);

        if (contState.IsConnected)
            Move();
    }

    void Move()
    {
        if (contState.Buttons.A == ButtonState.Pressed)
        {
            W_RL.motorTorque = torque;
            W_RR.motorTorque = torque;
        }
        else if(contState.Buttons.A == ButtonState.Released && W_RR.motorTorque > 0f && contState.Buttons.B == ButtonState.Released || contState.Triggers.Left == 0f)
        {
            W_RL.motorTorque = Mathf.Lerp(torque, 0f, 1f);
            W_RR.motorTorque = Mathf.Lerp(torque, 0f, 1f);
        }
        else if (contState.Buttons.B == ButtonState.Pressed)
        {
            W_RL.motorTorque = 0f;
            W_RR.motorTorque = 0f;
            W_RL.brakeTorque = brakingTorque;
            W_RR.brakeTorque = brakingTorque;
        }
        else if (contState.Buttons.B == ButtonState.Released && W_RR.brakeTorque > 0f)
        {
            W_RL.brakeTorque = 0f;
            W_RR.brakeTorque = 0f;
        }
        else if (contState.Triggers.Left > 0f)
        {
            W_RL.motorTorque = -torque * contState.Triggers.Left;
            W_RR.motorTorque = -torque * contState.Triggers.Left;
        }
        else if(contState.Triggers.Left == 0f && W_RL.motorTorque < 0f)
        {
            W_RL.motorTorque = Mathf.Lerp(-torque, 0f, 1f);
            W_RR.motorTorque = Mathf.Lerp(-torque, 0f, 1f);
        }

        float speed = rb.velocity.magnitude / lowestSteeringSpeed;
        float currentSteeringAngle = Mathf.Lerp(lowestSteeringAngle, highestSteeringAngle, speed);

        currentSteeringAngle *= contState.ThumbSticks.Left.X;

        W_FL.steerAngle = currentSteeringAngle;
        W_FR.steerAngle = currentSteeringAngle;
    }
}
