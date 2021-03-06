﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour 
{
    // Camera Movement Variables
    [Header("Camera Movement Settings")]
    [SerializeField] Camera mainCamera;
    [SerializeField] float scrollSpeed;
    [SerializeField] float zoomSpeed;

    // Mouse Position and Scroll Wheel Data
    float mousePosX;
    float mousePosY;
    float mouseScrollWheelDelta;

    void Update()
    {
        // Move the camera
        CameraMove();

        // Zoom the camera
        CameraZoom();
    }

    // Method to move the camera at the edges of the screen
    void CameraMove()
    {
        // Update the mouse position
        mousePosX = Input.mousePosition.x;
        mousePosY = Input.mousePosition.y;

        // Check to see where the mouse is, and move the camera left or right if needed
        if (mousePosX > Screen.width - .5f)
            mainCamera.transform.Translate(new Vector3(scrollSpeed * Time.deltaTime, 0f, 0f));
        else if (mousePosX < .5f)
            mainCamera.transform.Translate(new Vector3(-scrollSpeed * Time.deltaTime, 0f, 0f));

        // Check to see where the mouse is, and move the camera up or down if needed
        if (mousePosY > Screen.height - .5f)
            mainCamera.transform.Translate(new Vector3(0f, scrollSpeed * Time.deltaTime, 0f));
        else if (mousePosY < .5f)
            mainCamera.transform.Translate(new Vector3(0f, -scrollSpeed * Time.deltaTime, 0f));
    }

    // Method to zoom the camera in and out with the scroll wheel
    void CameraZoom()
    {
        // Update the scroll wheel delta
        mouseScrollWheelDelta = Input.GetAxis("Mouse ScrollWheel");

        // "Zoom" the camera in and out depending on which was the scroll wheel was moved
        if (mouseScrollWheelDelta > 0f)
            mainCamera.transform.Translate(new Vector3(0f, 0f, zoomSpeed * Time.deltaTime));
        else if (mouseScrollWheelDelta < 0f)
            mainCamera.transform.Translate(new Vector3(0f, 0f, -zoomSpeed * Time.deltaTime));
    }
}
