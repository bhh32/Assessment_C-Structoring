using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireToWheel : MonoBehaviour 
{
    [SerializeField] WheelCollider wheelCollider;

    void FixedUpdate()
    {
        UpdateWheelHeight(transform, wheelCollider);
    }

    void UpdateWheelHeight(Transform wheelTransform, WheelCollider collider)
    {
        Vector3 localPosition = wheelTransform.localPosition;

        WheelHit hit = new WheelHit();

        if (collider.GetGroundHit(out hit))
        {
            float hitY = collider.transform.InverseTransformPoint(hit.point).y;

            localPosition.y = hitY + collider.radius;
        }
        else
        {
            localPosition = Vector3.Lerp(localPosition, -Vector3.up * collider.suspensionDistance, .05f);

        }

        wheelTransform.localPosition = localPosition;
        wheelTransform.localRotation = Quaternion.Euler(0, collider.steerAngle, 0);
    }
}
