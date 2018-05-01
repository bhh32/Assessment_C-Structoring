using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour 
{
    public float maxWoodCapacity; // The max wood the facility can hold
    public float currentWoodCapacity; // How much wood is currently stored
    public float maxGoldCapacity; // The max gold the facility can hold
    public float currentGoldCapacity; // How much gold is currently stored

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Worker"))
        {
            var worker = other.GetComponent<Worker>();
            
            // Set the flag to false so that the agent dest can be reset
            worker.destSet = false;

            if (worker.currentOrders == Worker.Orders.UNLOAD && worker.carryinAmt > 0f)
            {
                // Unload the current resource into the correct place
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

                // Set the carrying amount to 0
                worker.carryinAmt = 0f;

                //If there's more to be harvested have the worker return
                if (worker.currentResource.GetComponent<Resources>().maxCapacity > 0f)
                {
                    worker.OnDestChange(worker.currentResource.transform.position);
                }
                //If not have het worker find the closest resource to the current one...
                else
                {
                    GameObject newResource = FindClosestResource(worker.currentResource);

                    // ... if that resource isn't too far away, have the worker go there...
                    if (Vector3.Distance(worker.transform.position, newResource.transform.position) < 10f)
                        worker.OnDestChange(newResource.transform.position);
                    // ... otherwise, set the worker to idle
                    else
                        worker.currentOrders = Worker.Orders.IDLE;
                }
            }
            else
            {
                // worker.currentOrders = Worker.Orders.IDLE;
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
