using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Storage : MonoBehaviour
{
    // Properties used because I wanted to update the UI
    [SerializeField] float goldStorageMax;
    public float GoldStorageMax
    {
        get { return goldStorageMax; }
    }

    [SerializeField] float goldStorageCurrent;
    public float CurrentGold
    {
        get { return goldStorageCurrent; }
        private set
        {
            goldStorageCurrent = value;
        }
    }

    [SerializeField] float woodStorageMax;
    public float WoodStorageMax
    {
        get { return woodStorageMax; }
    }

    [SerializeField] float woodStorageCurrent;
    public float CurrentWood
    {
        get { return woodStorageCurrent; }
        private set
        {
            woodStorageCurrent = value;
        }
    }

    UIManager updateUI;
    List<GameObject> workers;

    private void Start()
    {
        workers = new List<GameObject>();
        CurrentWood = 0f;
        CurrentGold = 0f;
        var tempObj = GameObject.Find("HUD Manager");
        updateUI = tempObj.GetComponent<UIManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Worker"))
        {
            if (!workers.Contains(other.gameObject))
            {
                workers.Add(other.gameObject);
            }

            foreach (GameObject obj in workers)
            {
                var orders = obj.GetComponent<WorkerOrders>();
                if (orders.CurrentWoodCarryingAmt > 0f || orders.CurrentGoldCarryingAmt > 0f 
                    && orders.CurrentOrders == BaseUnitOrders.Orders.UNLOAD)
                {
                    switch (orders.PreviousResource.tag)
                    {
                        case "Minable":
                            if (CurrentGold < GoldStorageMax)
                            {
                                CurrentGold += orders.CurrentGoldCarryingAmt;
                                if (CurrentGold > GoldStorageMax)
                                    CurrentGold = GoldStorageMax;

                                orders.CurrentGoldCarryingAmt = 0f;
                                orders.CurrentOrders = BaseUnitOrders.Orders.TAKE;
                                orders.TakeResource(orders.GetComponent<NavMeshAgent>(), orders.PreviousResource);
                            }
                            else
                            {
                                FindOtherFacility(orders);
                            }
                            break;

                        case "Choppable":
                            if (CurrentWood < WoodStorageMax)
                            {
                                CurrentWood += orders.CurrentWoodCarryingAmt;
                                if (CurrentWood > WoodStorageMax)
                                    CurrentWood = WoodStorageMax;

                                orders.CurrentWoodCarryingAmt = 0f;
                                orders.CurrentOrders = BaseUnitOrders.Orders.TAKE;
                                orders.TakeResource(orders.GetComponent<NavMeshAgent>(), orders.PreviousResource);
                            }
                            else
                            {
                                FindOtherFacility(orders);
                            }
                            break;
                    }                    
                    updateUI.OnResourceUpdate();
                }
            }
        }
    }

    void FindOtherFacility(WorkerOrders worker)
    {
        GameObject[] otherFacilities = GameObject.FindGameObjectsWithTag("Storage");
        var newFac = worker.FindClosestStorage(worker.GetComponent<NavMeshAgent>(), otherFacilities);

        if (newFac != null)
            worker.Unload(worker.GetComponent<NavMeshAgent>(), newFac);
        else
            worker.StartingMove(worker.GetComponent<NavMeshAgent>());
    }
}
