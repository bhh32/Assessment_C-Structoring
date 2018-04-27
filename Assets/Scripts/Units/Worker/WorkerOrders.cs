using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
// GO HERE TO SEE HOW TO BLOCK RAYCASTING WITH UI
// https://docs.unity3d.com/ScriptReference/EventSystems.EventSystem.IsPointerOverGameObject.html
public class WorkerOrders : BaseUnitOrders 
{
    UnitProperties properties;
    OrderSelection orderSelection;
    AutoHarvest harvestOrder;
    public GameObject PreviousResource
    { get { return previousResource; } }

    public float MaxCarryingAmt
    {
        get { return properties.maxCarryingAmt; }
    }

    public float CurrentCarryingAmt
    {
        get { return properties.currentCarryingAmt; }
        set 
        { 
            properties.currentCarryingAmt = value;
            Debug.Log(properties.currentCarryingAmt);
        }
    }

    [SerializeField] NavMeshAgent agent;
    [SerializeField] bool isSelected;
    public bool IsSelected
    {
        get { return isSelected; }
        set { isSelected = value; }
    }
    [SerializeField] Orders currentOrders;

    public Orders CurrentOrders
    {
        get { return currentOrders; }
        set { currentOrders = value; }
    }

    [SerializeField] GameObject selectedObj;
    public GameObject SelectedObj
    {
        get { return SelectedObj; }
        set { selectedObj = value; }
    }

    void Awake()
    {
        orderSelection = GameObject.Find("HUD Manager").GetComponent<OrderSelection>();
        CurrentOrders = Orders.EMPTY;
        properties.isSelected = false;
        properties.maxCarryingAmt = 5f;
        properties.currentCarryingAmt = 0f;
        properties.armor = 0f;
        base.StartingMove(agent);
    }
	
	// Update is called once per frame
	void FixedUpdate () 
    {
        if (isSelected)
        {
            if (!orderSelection.selectedUnits.Contains(gameObject))
                orderSelection.SelectedUnits = gameObject;
    
            selectedObj = TypeOfObj();


            IssueOrders();
        }
        else if (!isSelected)
        {
            if (orderSelection.selectedUnits.Contains(gameObject))
            {
                for (int i = 0; i < orderSelection.selectedUnits.Count; ++i)
                {
                    if (gameObject.Equals(orderSelection.selectedUnits[i]))
                    {
                        orderSelection.selectedUnits.RemoveAt(i);
                        break;
                    }
                }
            }

            AutoIssue();
        }
        Debug.DrawLine(transform.position, agent.destination);
        agent.isStopped = false;
	}

    void IssueOrders()
    {
        bool leftMouseClick = Input.GetMouseButtonUp(0);
        bool rightMouseclick = Input.GetMouseButtonUp(1);

        // Issue Specific Orders To The Worker
        switch (leftMouseClick)
        {
            case true:
                if (CurrentOrders == Orders.MOVE)
                    Move(agent);
                else if (CurrentOrders == Orders.BUILD)
                {
                    if (selectedObj.CompareTag("BuildHere"))
                        MoveToBuild(agent, selectedObj);
                    else
                        CurrentOrders = Orders.MOVE;
                }
                else if (CurrentOrders == Orders.TAKE)
                {
                    if (selectedObj.CompareTag("Minable") || selectedObj.CompareTag("Choppable"))
                        TakeResource(agent, selectedObj);
                    else
                        CurrentOrders = Orders.MOVE;
                }
                else if (CurrentCarryingAmt <= MaxCarryingAmt && CurrentOrders == Orders.UNLOAD)
                {
                    GameObject[] facs = GameObject.FindGameObjectsWithTag("Storage");
                    var closestStorage = FindClosestStorage(agent, facs);
                    CurrentOrders = Orders.MOVE;
                    Unload(agent, closestStorage);
                }

                break;

            case false:
                break;
        }

        // Issue Default Orders to the worker
        switch (rightMouseclick)
        {
            case true:
                if (selectedObj.CompareTag("BuildHere"))
                {
                    CurrentOrders = Orders.BUILD;
                    MoveToBuild(agent, selectedObj);
                }
                else if (selectedObj.CompareTag("Minable") || selectedObj.CompareTag("Choppable"))
                {
                    CurrentOrders = Orders.TAKE;
                   TakeResource(agent, selectedObj);
                }

                break;

            case false:
                break;
        }

        if (CurrentCarryingAmt <= MaxCarryingAmt && CurrentOrders == Orders.UNLOAD)
        {
            GameObject[] facs = GameObject.FindGameObjectsWithTag("Storage");
            var closestStorage = FindClosestStorage(agent, facs);
            CurrentOrders = Orders.MOVE;
            Unload(agent, closestStorage);
        }
    }

    void AutoIssue()
    {
        if (CurrentCarryingAmt <= MaxCarryingAmt && CurrentOrders == Orders.UNLOAD)
        {
            GameObject[] facs = GameObject.FindGameObjectsWithTag("Storage");
            var closestStorage = FindClosestStorage(agent, facs);
            CurrentOrders = Orders.MOVE;
            Unload(agent, closestStorage);
        }
    }

    GameObject TypeOfObj()
    {
        Camera mainCamera = FindObjectOfType<Camera>();
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        GameObject returnedObj = null;

        if (Physics.Raycast(ray, out hit, 100f))
            returnedObj = hit.collider.gameObject;

        return returnedObj;
    }
}
