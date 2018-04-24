using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script is from hunkell.com/blog/rts-style-unit-selection-in-unity-5/ tutorial

public class UnitSelection : MonoBehaviour 
{
    // Utility Variables and Methods
    static Texture2D whiteTexture;
    public static Texture2D WhiteTexture
    {
        get
        {
            if (whiteTexture == null)
            {
                whiteTexture = new Texture2D(1, 1);
                whiteTexture.SetPixel(0, 0, Color.white);
                whiteTexture.Apply();
            }

            return whiteTexture;
        }
    }

    [SerializeField] Camera mainCamera;

    public static void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, WhiteTexture);
        GUI.color = Color.white;
    }

    public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        // Top
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        // Left
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        // Right
        DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        // Bottom
        DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }

    public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
    {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;

        // Calculate Corners
        var topLeft = Vector3.Min(screenPosition1, screenPosition2);
        var bottomRight = Vector3.Max(screenPosition1, screenPosition2);

        // Create Rect
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2)
    {
        var v1 = Camera.main.ScreenToViewportPoint(screenPosition1);
        var v2 = Camera.main.ScreenToViewportPoint(screenPosition2);
        var min = Vector3.Min(v1, v2);
        var max = Vector3.Max(v1, v2);
        min.z = camera.nearClipPlane;
        max.z = camera.farClipPlane;

        var bounds = new Bounds();
        bounds.SetMinMax(min, max);
        return bounds;
    }

    // Selection Variables and Methods
    bool isSelecting = false;
    Vector3 mousePostion1;
    [SerializeField] GameObject[] units;


    void Update()
    {
//        if (Input.GetMouseButtonDown(0))
//        {
//            isSelecting = true;
//            mousePostion1 = Input.mousePosition;
//
//            foreach (GameObject obj in units)
//            {
//                IsWithinSelectionBounds(obj);
//            }
//        }
//        else if (Input.GetMouseButtonUp(0))
//            isSelecting = false;


    }

    void OnGUI()
    {
        if (isSelecting)
        {
            // Create a rect from both mouse positions
            var rect = GetScreenRect(mousePostion1, Input.mousePosition);
            DrawScreenRect(rect, new Color(.8f, .8f, .95f, .25f));
            DrawScreenRectBorder(rect, 2f, new Color(.8f, .8f, .95f));
        }
    }

//    public bool IsWithinSelectionBounds(GameObject unit)
//    {
//        if (!isSelecting)
//            return false;
//
//        var viewportBounds = GetViewportBounds(mainCamera, mousePostion1, Input.mousePosition);
//
//        return viewportBounds.Contains(mainCamera.WorldToViewportPoint(unit.transform.position));
//    }
}
