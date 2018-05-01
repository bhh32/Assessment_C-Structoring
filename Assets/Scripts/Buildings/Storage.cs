using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour 
{
    public float maxWoodCapacity; // The max wood the facility can hold
    public float currentWoodCapacity; // How much wood is currently stored
    public float maxGoldCapacity; // The max gold the facility can hold
    public float currentGoldCapacity; // How much gold is currently stored
    Worker worker;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Worker"))
        {
            worker = other.GetComponent<Worker>();
            
            // Set the flag to false so that the agent dest can be reset
            worker.destSet = false;

            if (worker.currentOrders == Worker.Orders.UNLOAD && worker.carryinAmt > 0f)
            {
                if (worker.currentResource != null)
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
                        Debug.Log(string.Format("{0} maxCapacity: {1}", worker.currentResource.name, worker.currentResource.GetComponent<Resources>().maxCapacity));
                        worker.OnDestChange(worker.currentResource.transform.position);
                    }
                    //If not have het worker find the closest resource to the current one...
                    else if (worker.currentResource.GetComponent<Resources>().maxCapacity <= 0f)
                    {
                    
                        // ... if that resource isn't too far away, have the worker go there...
                        if (Vector3.Distance(worker.transform.position, worker.currentResource.transform.position) < 10f)
                            worker.OnDestChange(worker.currentResource.transform.position);
                        // ... otherwise, set the worker to idle
                        else
                            worker.currentOrders = Worker.Orders.IDLE;
                    }
                }
            }
            else
            {
                // worker.currentOrders = Worker.Orders.IDLE;
            }
        }
    }


}
