using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resources : MonoBehaviour 
{
    public float maxCapacity; // Max yield that the resource holds
	
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Worker"))
        {
            var worker = other.GetComponent<Worker>();

            /** Set the destSet flag false so the worker can move to the storage facility **/
            worker.destSet = false;

            /** Check to ensure the worker is supposed to be gathering this resource
                and that they are supposed to be gathering resources in general **/
            if (worker.currentResource == gameObject && worker.currentOrders == Worker.Orders.CHOP 
                || worker.currentOrders == Worker.Orders.MINE)
            {
                /** Check to make sure the worker's carrying capacity is less than
                    the max capacity of the resource **/
                    if (worker.carryingCapacity < maxCapacity)
                    {
                        maxCapacity -= worker.carryingCapacity; // Take away from the max capacity
                        worker.carryinAmt = worker.carryingCapacity; // set the yield to the capacity

                        worker.currentOrders = Worker.Orders.UNLOAD;
                        worker.OnDestChange(FindClosestStorageFac().transform.position);
                    }
                    /** Otherwise, we set the yield to the maxCapacity,
                        change the orders to UNLOAD, send the worker to the nearest
                        storage facility, and set this gameObject to inactive **/
                    else
                    {
                        worker.carryinAmt = maxCapacity;

                        worker.currentOrders = Worker.Orders.UNLOAD;
                        worker.OnDestChange(FindClosestStorageFac().transform.position);

                        gameObject.SetActive(false);
                    }


            }
        }
    }

    GameObject FindClosestStorageFac()
    {
        GameObject[] storageFacs = GameObject.FindGameObjectsWithTag("Storage");
        GameObject closestFac = null;
        float dist = 1000f;

        foreach (GameObject facs in storageFacs)
        {
            float tempDist = Vector3.Distance(facs.transform.position, gameObject.transform.position);
            if (tempDist < dist)
            {
                closestFac = facs;
                dist = tempDist;
            }
        }

        return closestFac;
    }
}
