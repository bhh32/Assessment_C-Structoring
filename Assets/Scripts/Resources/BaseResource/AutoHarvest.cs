using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AutoHarvest : MonoBehaviour 
{
    void OnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.collider.CompareTag("Choppable") || hit.collider.CompareTag("Minable"))
            {
                var selectedWorkers = GameObject.FindGameObjectsWithTag("Worker");

                foreach (GameObject workers in selectedWorkers)
                {
                    var workOrder = workers.GetComponent<WorkerOrders>();
                    if (workOrder.IsSelected)
                    {
                        workOrder.SelectedObj = hit.collider.gameObject;
                        workOrder.CurrentOrders = BaseUnitOrders.Orders.TAKE;
                        workOrder.GetComponent<NavMeshAgent>().SetDestination(hit.collider.transform.position);
                    }
                }
            }
        }
    }
}
