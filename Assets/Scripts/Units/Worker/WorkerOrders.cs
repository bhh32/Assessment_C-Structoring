using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
// GO HERE TO SEE HOW TO BLOCK RAYCASTING WITH UI
// https://docs.unity3d.com/ScriptReference/EventSystems.EventSystem.IsPointerOverGameObject.html
public class WorkerOrders : BaseUnitOrders 
{
    UnitProperties properties;
    [SerializeField] OrderSelection orderSelection;

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
    { get { return isSelected; } }
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
        CurrentOrders = Orders.EMPTY;
        properties.isSelected = false;
        properties.maxCarryingAmt = 5f;
        properties.currentCarryingAmt = 0f;
        properties.armor = 0f;
        base.StartingMove(agent);
    }
	
	// Update is called once per frame
	void Update () 
    {
        if (isSelected)
        {
            selectedObj = TypeOfObj();
            orderSelection.SelectedUnit = selectedObj;
            IssueOrders();
        } 
        agent.isStopped = false;
	}

    void IssueOrders()
    {
        bool leftMouseClick = Input.GetMouseButtonUp(0);

        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        if (leftMouseClick && CurrentOrders == Orders.MOVE)
            Move(agent);
        else if (leftMouseClick && CurrentOrders == Orders.BUILD)
        {
            if (selectedObj.CompareTag("BuildHere"))
                MoveToBuild(agent, selectedObj);
            else
                CurrentOrders = Orders.MOVE;
        }
        else if (leftMouseClick && CurrentOrders == Orders.TAKE)
        {
            if (selectedObj.CompareTag("Minable") || selectedObj.CompareTag("Choppable"))
                TakeResource(agent, selectedObj);
            else
                CurrentOrders = Orders.MOVE;        
        }
        else if (leftMouseClick || CurrentCarryingAmt <= MaxCarryingAmt && CurrentOrders == Orders.UNLOAD)
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
