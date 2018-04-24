using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseResource : MonoBehaviour 
{
    [SerializeField] protected float maxAmt;
    [SerializeField] protected float harvestAmt;

    public float MaxAmt{ get { return maxAmt; } }
    List<GameObject> workers;
    GameObject currentWorker;

    public delegate void TakeResource();
    public TakeResource OnResourceMined;

    void Awake()
    {
        workers = new List<GameObject>();
        OnResourceMined += Mine;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Worker"))
        {
            workers.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Worker"))
            workers.Remove(other.gameObject);
    }

    public void Mine()
    {
        StartCoroutine(Mining(harvestAmt));
    }


    IEnumerator Mining(float amt)
    {
        float harvested = 0f;

        while (harvested < amt && MaxAmt > 0f)
        {
            harvested++;
            foreach (GameObject worker in workers)
            {
                
                var order = worker.GetComponent<WorkerOrders>();
                order.CurrentCarryingAmt++;
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
            Debug.Log("CurrentCarryingAmt: " + orders.CurrentCarryingAmt);
            if (orders.CurrentCarryingAmt >= harvested)
                orders.CurrentOrders = BaseUnitOrders.Orders.UNLOAD;
            else
            {
                Mine();
            }
        }
    }
}
