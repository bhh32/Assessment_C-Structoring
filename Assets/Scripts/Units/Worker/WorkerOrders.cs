using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WorkerOrders : BaseUnitOrders 
{
    UnitProperties properties;

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

            IssueOrders();
        }            
	}

    void IssueOrders()
    {
        bool leftMouseClick = Input.GetMouseButtonUp(0);

        if (leftMouseClick && CurrentOrders == Orders.MOVE)
            Move(agent);
        else if (leftMouseClick && CurrentOrders == Orders.ATTACK)
        {
            if (selectedObj.CompareTag("Enemy"))
                Attack();
            else
                CurrentOrders = Orders.EMPTY;
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

    void Move()
    {
        base.Move(agent);
    }

    void Attack()
    {
        base.Attack(agent, selectedObj);
    }

    void Explore()
    {
        base.Explore();
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
