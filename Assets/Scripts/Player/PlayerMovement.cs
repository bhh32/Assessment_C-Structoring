using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class PlayerMovement : MonoBehaviour 
{
    [SerializeField] Rigidbody rb;
    [SerializeField] float speed = 10f;
	[SerializeField] float pitchSpeed = 10f;
	[SerializeField] float yawSpeed = 10f;
	PlayerIndex playerCont;
	GamePadState controllerState;

	void Awake()
	{
		playerCont = PlayerIndex.One;
		rb.inertiaTensor = new Vector3(1, 1, 1);
	}

	// Update is called once per frame
	void Update () 
    {
		controllerState = GamePad.GetState (playerCont);

		if (controllerState.IsConnected) 
		{
			

			if (controllerState.Buttons.A == ButtonState.Pressed) 
				rb.AddRelativeForce (0f, 0f, -speed * Time.deltaTime);
			else if (controllerState.Buttons.B == ButtonState.Pressed && rb.velocity.z > 0f) 
				rb.AddRelativeForce(0f, 0f, speed * Time.deltaTime);

			if (controllerState.ThumbSticks.Left.X > .5f)
				rb.AddRelativeTorque(0f, 0f, yawSpeed * Time.deltaTime, ForceMode.Force);
			else if (controllerState.ThumbSticks.Left.X < -.5f)
				rb.AddRelativeTorque(0f, 0f, -yawSpeed * Time.deltaTime, ForceMode.Force);

			if (controllerState.ThumbSticks.Left.Y > .5f)
				rb.AddRelativeTorque(-pitchSpeed * Time.deltaTime, 0f, 0f, ForceMode.Force);
			else if (controllerState.ThumbSticks.Left.Y < -.5f)
				rb.AddRelativeTorque(pitchSpeed * Time.deltaTime,0f, 0f, ForceMode.Force);

		}
	}
}
