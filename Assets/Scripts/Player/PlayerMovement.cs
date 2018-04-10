using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour 
{
    [SerializeField] Rigidbody rb;
    [SerializeField] float speed = 10f;
	
	// Update is called once per frame
	void Update () 
    {
        // Direction variables
        float moveForward = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        float moveRight = Input.GetAxis("Horizontal") * speed * Time.deltaTime;

        // Movement Vector
        Vector3 move = new Vector3(moveRight, 0f, moveForward);

        transform.Translate(move);
	}
}
