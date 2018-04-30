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

    [SerializeField] float debugCurrentCarrying;

    public GameObject thisWorkersPreviousResource;
    bool orderIssued;

    public GameObject PreviousResource
    { get { return previousResource; } }

    public float MaxGoldCarryingAmt
    { get { return properties.maxGoldCarryingAmt; } }

    public float CurrentGoldCarryingAmt
    {
        get { return properties.currentGoldCarryingAmt; }

        set
        {
            properties.currentGoldCarryingAmt = value;
        }
    }

    public float MaxWoodCarryingAmt
    {
        get { return properties.maxWoodCarryingAmt; }
    }

    public float CurrentWoodCarryingAmt
    {
        get { return properties.currentWoodCarryingAmt; }
        set 
        { 
            properties.currentWoodCarryingAmt = value;
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
        thisWorkersPreviousResource = null;
        CurrentOrders = Orders.EMPTY;
        properties.isSelected = false;
        properties.maxGoldCarryingAmt = 5f;
        properties.currentGoldCarryingAmt = 0f;
        properties.maxWoodCarryingAmt = 5f;
        properties.currentWoodCarryingAmt = 0f;
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
            if(CurrentGoldCarryingAmt > 0f || CurrentWoodCarryingAmt > 0f && thisWorkersPreviousResource == null)
                thisWorkersPreviousResource = previousResource;
                

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
                else if (CurrentOrders == Orders.UNLOAD && CurrentGoldCarryingAmt > 0f || CurrentWoodCarryingAmt > 0f)
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

        if(!orderIssued)
        AutoIssue();
    }

    void AutoIssue()
    {
        orderIssued = true;

        if (thisWorkersPreviousResource != null)
        {
            if (thisWorkersPreviousResource.CompareTag("Minable"))
            {
                if (CurrentGoldCarryingAmt <= MaxGoldCarryingAmt && CurrentOrders == Orders.UNLOAD)
                {
                    FindLocalStorage();
                }
            }
            else if (thisWorkersPreviousResource.CompareTag("Choppable"))
            {
                FindLocalStorage();
            }
        }
    }

    void FindLocalStorage()
    {
        GameObject[] facs = GameObject.FindGameObjectsWithTag("Storage");
        var closestStorage = FindClosestStorage(agent, facs);
        CurrentOrders = Orders.MOVE;
        Unload(agent, closestStorage);
        orderIssued = false;
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
