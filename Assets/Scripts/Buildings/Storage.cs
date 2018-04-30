using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour 
{
    public float maxWoodCapacity;
    public float currentWoodCapacity;
    public float maxGoldCapacity;
    public float currentGoldCapacity;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Worker"))
        {
            var worker = other.GetComponent<Worker>();

            worker.destSet = false;

            if (worker.currentOrders == Worker.Orders.UNLOAD && worker.carryinAmt > 0f)
            {
                if (worker.currentResource.CompareTag("Choppable"))
                {
                    currentWoodCapacity += worker.carryinAmt;
                    worker.currentOrders = Worker.Orders.CHOP;
                }
                else if (worker.currentResource.CompareTag("Minable"))
                {
                    currentGoldCapacity += worker.carryinAmt;
                    worker.currentOrders = Worker.Orders.MINE;
                }

                worker.carryinAmt = 0f;

                if (worker.currentResource.GetComponent<Resources>().maxCapacity > 0f)
                {
                    worker.OnDestChange(worker.currentResource.transform.position);
                }
                else
                {
                    GameObject newResource = FindClosestResource(worker.currentResource);

                    if (Vector3.Distance(worker.transform.position, newResource.transform.position) < 10f)
                        worker.OnDestChange(newResource.transform.position);
                    else
                        worker.currentOrders = Worker.Orders.IDLE;
                }
            }
            else
            {
                worker.currentOrders = Worker.Orders.IDLE;
            }
        }
    }

    GameObject FindClosestResource(GameObject resource)
    {
        GameObject[] resources = null;

        if (resource.CompareTag("Choppable"))
            resources = GameObject.FindGameObjectsWithTag("Choppable");
        else if (resource.CompareTag("Minable"))
            resources = GameObject.FindGameObjectsWithTag("Minable");
        
        GameObject closestFac = null;
        float dist = 1000f;

        foreach (GameObject facs in resources)
        {
            float tempDist = Vector3.Distance(gameObject.transform.position, gameObject.transform.position);
            if (tempDist < dist)
            {
                closestFac = facs;
                dist = tempDist;
            }
        }

        return closestFac;
    }
}
