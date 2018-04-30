using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Worker : MonoBehaviour 
{
    // Delegate to update worker unit's destination when needed
    public delegate void NewDestination(Vector3 destination);
    public NewDestination OnDestChange;

    // Enum to issue orders
    public enum Orders
    {
        WALK, CHOP, MINE, UNLOAD, BUILD, IDLE, ERROR
    };
        
    public Orders currentOrders; // The worker's current orders
    public bool isSelected = false; // Sets whether the worker is selected or not
    [SerializeField] NavMeshAgent agent; // The workers NavMeshAgent
    Vector3 clickPoint; // The point on the screen that the player clicked
    public GameObject currentResource; // The resource the worker is set to currently harvest
    public float carryingCapacity; // The worker's max carrying capacity
    public float carryinAmt; // The worker's current carrying amount
    public bool destSet; // True if a destination is set, false if it's not

	// Use this for initialization
	void Start () 
    {
        // Subscribe to the delegate
        OnDestChange += ChangeDest;	
	}
	
	// Update is called once per frame
	void Update () 
    {
        // Get if the left mouse button is clicked
        bool isLeftClicked = Input.GetMouseButtonUp(0);

        // If the left mouse button is clicked update the clickPoint
        if(isLeftClicked)
            SetClickPoint();

        // If the worker is selected issue orders
        if (isSelected)
            IssueOrders();
	}

    void IssueOrders()
    {
        // Checks the workers current orders
        switch (currentOrders)
        {
            case Orders.WALK:
                if(!destSet) // if a destination isn't set walk
                    Walk();
                break;
            case Orders.CHOP:
            case Orders.MINE:
                if(!destSet) // if a destination isn't set walk to the resource
                    Walk();
                break;
            case Orders.UNLOAD:
            case Orders.BUILD:
                Walk(); // Walk to the building foundation
                break;
            case Orders.IDLE: // Idle is to do nothing
                break;
            case Orders.ERROR:
                Debug.LogError("There Was An Error In The IssueOrders Method!");
                break;
        }
    }

    void SetClickPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        int layerHit; // To record what layer the object clicked is on

        if (Physics.Raycast(ray, out hit, 100f))
        {
            layerHit = hit.collider.gameObject.layer;

            // Set the orders and specific variables based on the object that was clicked's layer
            switch (layerHit)
            {
                case 8:
                    currentOrders = Orders.WALK;
                    break;
                case 9:
                    currentOrders = Orders.CHOP;
                    currentResource = hit.collider.gameObject;
                    break;
                case 10:
                    currentOrders = Orders.MINE;
                    currentResource = hit.collider.gameObject;
                    break;
                case 11:
                    currentOrders = Orders.BUILD;
                    break;
                case 12:
                    currentOrders = Orders.UNLOAD;
                    break;
                case 13:
                    currentOrders = Orders.IDLE;
                    if (hit.collider.gameObject == gameObject)
                        isSelected = true;
                    else
                        isSelected = false;
                    break;
                default:
                    isSelected = false;
                    break;
            }

            if(destSet)
                destSet = false;
            clickPoint = hit.point;
        }
    }

    void Walk()
    {
        destSet = true;
        agent.SetDestination(clickPoint);        
    }

    void ChangeDest(Vector3 destination)
    {
        destSet = true;

        // This allows the worker to move between the resource and storage if it's selected
        if (isSelected)
        {            
            clickPoint = destination; 
        }
        // This allows the worker to move between the resource and storage if it isn't selected
        else
        {
            agent.SetDestination(destination);
        }
    }
}
