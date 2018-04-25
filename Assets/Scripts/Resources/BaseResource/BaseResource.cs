using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseResource : MonoBehaviour 
{
    [SerializeField] protected float maxAmt;
    [SerializeField] protected float harvestAmt;
    ScriptableInstCreator listOfResources;

    public float MaxAmt{ get { return maxAmt; } }
    List<GameObject> workers;
    GameObject currentWorker;

    public delegate void TakeResource();
    public TakeResource OnResourceHarvested;

    void Awake()
    {
        workers = new List<GameObject>();
        OnResourceHarvested += Harvest;
    }

    void Start()
    {
        //listOfResources.resourceLists.OnAddResource(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        WorkerOrders orders = null;

        if (other.CompareTag("Worker"))
        {
            orders = other.gameObject.GetComponent<WorkerOrders>();            
        
            // Redundant, but ensures that if worker walks over mine and is chopping
            // wood it doesn't start mining instead, and vice versa.
            if (MaxAmt > 0 && gameObject.CompareTag("Minable") 
                && orders.CurrentOrders == BaseUnitOrders.Orders.TAKE)
            {
                OnResourceHarvested();
                workers.Add(other.gameObject);
            }
            else if(MaxAmt > 0 && gameObject.CompareTag("Choppable") 
                && orders.CurrentOrders == BaseUnitOrders.Orders.TAKE)
            {
                OnResourceHarvested();
                workers.Add(other.gameObject);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (workers.Contains(other.gameObject))
            workers.Remove(other.gameObject);
    }

    public void Harvest()
    {
        StartCoroutine(Harvesting(harvestAmt));
    }


    IEnumerator Harvesting(float amt)
    {
        float harvested = 0f;

        while (harvested < amt && MaxAmt > 0f)
        {   
            foreach (GameObject worker in workers)
            {   
                harvested++;
                var order = worker.GetComponent<WorkerOrders>();
                order.CurrentCarryingAmt = harvested;
            }

            if (harvested > maxAmt)
            {
                harvested = maxAmt;
                maxAmt = 0f;
                break;
            }
            else if (harvested < amt)
            {
                maxAmt--;
            }

            yield return new WaitForSeconds(1f);
        }

        foreach (GameObject worker in workers)
        {
            var orders = worker.GetComponent<WorkerOrders>();
            if (orders.CurrentCarryingAmt <= orders.MaxCarryingAmt || MaxAmt == 0f)
            {
                orders.CurrentOrders = BaseUnitOrders.Orders.UNLOAD;

                if (MaxAmt == 0f)
                {
                    GameObject[] storage = GameObject.FindGameObjectsWithTag("Storage");

                    GameObject closestStorage = orders.FindClosestStorage(worker.GetComponent<NavMeshAgent>(), storage);

                    worker.GetComponent<NavMeshAgent>().SetDestination(closestStorage.transform.position);
                    orders.CurrentOrders = BaseUnitOrders.Orders.UNLOAD;
                    //listOfResources.resourceLists.OnRemoveResource(gameObject);
                    gameObject.SetActive(false);
                }
                
            }
        }
    }
}
