using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OrderSelection : MonoBehaviour
{
    [SerializeField] GameObject selectedUnit;

    [SerializeField] public List<GameObject> selectedUnits; 
    public GameObject SelectedUnits
    {
        get { return selectedUnit; }
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
        //selectedUnits = null;
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
        //Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        //RaycastHit hit;

        //if(Physics.Raycast(mouseRay, out hit, 100f, 5))
        //{
            foreach (GameObject unit in selectedUnits)
            {
                if (unit.CompareTag("Worker"))
                {
                    unit.GetComponent<WorkerOrders>().CurrentOrders = BaseUnitOrders.Orders.TAKE;
                    currentOrders = unit.GetComponent<WorkerOrders>().CurrentOrders;
                }
            }
        //}
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
     

    public GameObject[] ReturnSelectedUnits()
    {
        return selectedUnits.ToArray();
    }
}
