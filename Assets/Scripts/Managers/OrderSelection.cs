using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OrderSelection : MonoBehaviour
{
    [SerializeField] GameObject selectedUnit;
    //public GameObject SelectedUnit
    //{
    //    get { return selectedUnit; }
    //    set { selectedUnit = value; }
    //}

    public List<GameObject> selectedUnits;
    public GameObject SelectedUnits
    {
        get { return selectedUnit;  }
        set
        {
            selectedUnit = value;
            selectedUnits.Add(selectedUnit);
        }
    }

    [SerializeField] BaseUnitOrders.Orders currentOrders;

    void Awake()
    {
        selectedUnit = null;
        selectedUnits = new List<GameObject>();
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
        foreach(GameObject unit in selectedUnits)
        {
            if (unit.CompareTag("Worker"))
            {
                var newOrder = unit.GetComponent<WorkerOrders>().CurrentOrders = BaseUnitOrders.Orders.TAKE;
                currentOrders = newOrder;
            }           
        }
        
    }

    public void Build()
    {
        foreach (GameObject unit in selectedUnits)
        {
            if (unit.CompareTag("Worker"))
            {
                unit.GetComponent<WorkerOrders>().CurrentOrders = BaseUnitOrders.Orders.BUILD;
            }
        }
    }

    // Helper Method
    public GameObject[] ReturnSelectedUnits()
    {
        return selectedUnits.ToArray();
    }
}
