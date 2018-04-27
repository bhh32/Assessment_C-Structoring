using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public GameObject currentlySelected;
    GameObject[] workers;
    List<WorkerOrders> workerOrders;

	// Use this for initialization
	void Start ()
    {
        workerOrders = new List<WorkerOrders>();
        workers = GameObject.FindGameObjectsWithTag("Worker");

        for (int i = 0; i < workers.Length; ++i)
        {
            workerOrders.Add(workers[i].GetComponent<WorkerOrders>());
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("You Selected Something!");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                currentlySelected = hit.collider.gameObject;
                if (currentlySelected.CompareTag("Worker"))
                {
                    foreach (WorkerOrders worker in workerOrders)
                    {
                        if (worker.gameObject == currentlySelected)
                            worker.IsSelected = true;
                        else
                            worker.IsSelected = false;
                    }
                }
                else if (currentlySelected.CompareTag("Minable") || currentlySelected.CompareTag("Choppable"))
                {
                    currentlySelected.GetComponent<WorkerOrders>().CurrentOrders = BaseUnitOrders.Orders.TAKE;
                }
                else if (currentlySelected.CompareTag("Ground"))
                {
                    for (int i = 0; i < workerOrders.Count + 1; ++i)
                    {
                        if (workerOrders[i].gameObject == currentlySelected)
                            workerOrders[i].CurrentOrders = BaseUnitOrders.Orders.MOVE;
                    }
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            foreach(WorkerOrders worker in workerOrders)
            {
                worker.IsSelected = false;
            }
        }
	}
}
