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
    UIManager uIManager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Worker"))
        {
            // Get the Worker script
            worker = other.GetComponent<Worker>();
            
            // Set the flag to false so that the agent dest can be reset
            worker.destSet = false;
            worker.hasOrders = true;

            if (worker.currentOrders == Worker.Orders.UNLOAD && worker.carryinAmt > 0f)
            {
                /** This ensures that the resource is dropped off 
                    even if there's no longer a current resource 
                    to return to. **/
                if(worker.previousResource != null)
                {
                    if (worker.previousResource.CompareTag("Choppable"))
                    {
                        currentWoodCapacity += worker.carryinAmt;
                        worker.currentOrders = Worker.Orders.CHOP;
                    }
                    else if (worker.previousResource.CompareTag("Minable"))
                    {
                        currentGoldCapacity += worker.carryinAmt;
                        worker.currentOrders = Worker.Orders.MINE;
                    }

                    // This effectively decouples the UI from the Storage Facility
                    if (GameObject.Find("HUD Manager") != null)
                    {
                        var hudManager = GameObject.FindGameObjectWithTag("HUDManager");
                        hudManager.GetComponent<UIManager>().OnResourceChange();
                    }
                }
                
                // Set the carrying amount to 0
                worker.carryinAmt = 0f;

                // If there's more to be harvested have the worker return
                if (worker.currentResource != null && worker.currentResource.GetComponent<Resources>().maxCapacity > 0f)
                    worker.OnDestChange(worker.currentResource.transform.position);
                // Otherwise set the worker to IDLE
                else
                {
                    worker.hasOrders = false;
                    worker.currentOrders = Worker.Orders.IDLE;
                }
                
            }
        }
    }


}
