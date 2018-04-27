using UnityEngine;
using UnityEngine.AI;

class Storage : MonoBehaviour
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
    private float WoodStorageMax
    {
        get { return woodStorageMax; }
        set
        {
            woodStorageMax = value;
        }
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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Worker"))
        {
            var orders = other.GetComponent<WorkerOrders>();
            if (orders.CurrentCarryingAmt <= orders.MaxCarryingAmt && 
                orders.CurrentCarryingAmt > 0f && orders.CurrentOrders == BaseUnitOrders.Orders.UNLOAD)
            {
                switch(orders.PreviousResource.tag)
                {
                    case "Minable":
                        if (CurrentGold < GoldStorageMax)
                        {
                            CurrentGold = orders.CurrentCarryingAmt;

                            orders.CurrentCarryingAmt = 0f;
                            if (CurrentGold > GoldStorageMax)
                                CurrentGold = GoldStorageMax;

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
                            CurrentWood = orders.CurrentCarryingAmt;

                            orders.CurrentCarryingAmt = 0f;
                            if (CurrentWood > WoodStorageMax)
                                CurrentWood = WoodStorageMax;

                            orders.CurrentOrders = BaseUnitOrders.Orders.TAKE;
                            orders.TakeResource(orders.GetComponent<NavMeshAgent>(), orders.PreviousResource);
                        }
                        else
                        {
                            FindOtherFacility(orders);
                        }
                        break;
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
