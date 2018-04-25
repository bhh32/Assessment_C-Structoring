using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OrderSelection : MonoBehaviour
{
    [SerializeField] GameObject selectedUnit;
    public GameObject SelectedUnit
    {
        get { return selectedUnit; }
        set { selectedUnit = value; }
    }

    [SerializeField] BaseUnitOrders.Orders currentOrder;
    [SerializeField] GameObject hoveredObj;

    [SerializeField] GameObject[] selectedUnits;
    public GameObject[] SelectedUnits
    {
        get { return selectedUnits; }
        set
        {
            for (int i = 0; i < selectedUnits.Length; i++)
            {
                selectedUnits[i] = value[i];
            }
        }
    }

    void Awake()
    {
        selectedUnit = null;
        selectedUnits = null;
    }

    bool ValidSelection()
    {
        if (selectedUnit != null)
        {
            return true;
        }

        if (selectedUnits != null)
            return true;

        return false;
    }

    public void Harvest()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(mouseRay, out hit, 100f, 5))
        {
            hoveredObj = hit.collider.gameObject;
            if (selectedUnit.GetComponent<WorkerOrders>() != null)
            {
                selectedUnit.GetComponent<WorkerOrders>().CurrentOrders = BaseUnitOrders.Orders.EMPTY;
            
                currentOrder = selectedUnit.GetComponent<WorkerOrders>().CurrentOrders;
            }
                if (ValidSelection() && selectedUnit.CompareTag("Worker"))
                {
                    selectedUnit.GetComponent<WorkerOrders>().CurrentOrders = BaseUnitOrders.Orders.TAKE;
                    currentOrder = selectedUnit.GetComponent<WorkerOrders>().CurrentOrders;
                }
                else if (ValidSelection())
                {
                    foreach (GameObject unit in selectedUnits)
                    {
                        if (unit.CompareTag("Worker"))
                        {
                            unit.GetComponent<WorkerOrders>().CurrentOrders = BaseUnitOrders.Orders.TAKE;
                        }
                    }
                }           
        }
    }

    public void Build()
    {
        if (ValidSelection() && selectedUnit.CompareTag("Worker"))
        {
            selectedUnit.GetComponent<WorkerOrders>().CurrentOrders = BaseUnitOrders.Orders.BUILD;
        }
        else if (ValidSelection())
        {
            foreach (GameObject unit in selectedUnits)
            {
                if (unit.CompareTag("Worker"))
                {
                    unit.GetComponent<WorkerOrders>().CurrentOrders = BaseUnitOrders.Orders.BUILD;
                }
            }
        }
    }
        
}
