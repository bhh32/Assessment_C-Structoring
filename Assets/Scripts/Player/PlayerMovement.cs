using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour 
{
    [SerializeField] Rigidbody rb;
    [SerializeField] float speed = 10f;
    [SerializeField] float rotate = 20f;
    [SerializeField] float turnVal = 40f;
	
	// Update is called once per frame
	void Update () 
    {
        // Direction variables
        float moveForward = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        float moveRight = Input.GetAxis("Horizontal") * turnVal * .02f;
        float rotateVal = Input.GetAxis("Horizontal") * rotate * Time.deltaTime;

        // Movement Vector
        Vector3 move = new Vector3(0f, moveRight, moveForward);
        Vector3 turn = new Vector3(0f, 0f, -rotateVal);

        // Applied Force
        rb.AddForce(move);
        rb.AddTorque(turn);
	}
}
