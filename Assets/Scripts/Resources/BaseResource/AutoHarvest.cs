using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AutoHarvest : MonoBehaviour 
{
    public delegate void RemoveUnit(GameObject removedUnit);
    public RemoveUnit OnRemoveUnit;


    [SerializeField] BaseUnitOrders.Orders currentOrders;
  
    // Fix this later, it's just to get it working
    OrderSelection orderSelection;

    void Awake()
    {
        orderSelection = GameObject.FindGameObjectWithTag("HUDManager").GetComponent<OrderSelection>();
    }

    void FixedUpdate()
    {
        if (Input.GetMouseButtonUp(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.collider.CompareTag("Choppable") || hit.collider.CompareTag("Minable"))
                {
                    orderSelection.Harvest();

                    foreach (GameObject worker in orderSelection.ReturnSelectedUnits())
                    {
                        worker.GetComponent<NavMeshAgent>().SetDestination(gameObject.transform.position);
                        currentOrders = worker.GetComponent<WorkerOrders>().CurrentOrders;
                    }
                }
            }
        }
    }
}
