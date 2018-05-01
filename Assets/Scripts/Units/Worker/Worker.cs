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
    public bool hasOrders;
    public bool isSelected = false; // Sets whether the worker is selected or not
    public bool IsSelected
    { get { return isSelected; }
      set { isSelected = value; }
    }
    [SerializeField] NavMeshAgent agent; // The workers NavMeshAgent
    Vector3 clickPoint; // The point on the screen that the player clicked
    public GameObject currentResource; // The resource the worker is set to currently harvest
    public GameObject previousResource;
    public float carryingCapacity; // The worker's max carrying capacity
    public float carryinAmt; // The worker's current carrying amount
    public bool destSet; // True if a destination is set, false if it's not

	// Use this for initialization
	void Start () 
    {
        currentOrders = Orders.IDLE;
        hasOrders = false;

        // Subscribe to the delegate
        OnDestChange += ChangeDest;	
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            isSelected = false;

        // Get if the left mouse button is clicked
        bool isLeftClicked = Input.GetMouseButtonUp(0);

        // If the left mouse button is clicked update the clickPoint
        if(isLeftClicked)
            SetClickPoint();

        // If the worker is selected issue orders
        if (isSelected && !hasOrders)
            IssueOrders();
	}

    void IssueOrders()
    {
        // Checks the workers current orders
        switch (currentOrders)
        {
            case Orders.WALK:
                hasOrders = false;
                destSet = false;
                agent.SetDestination(clickPoint);
                break;
            case Orders.CHOP:
            case Orders.MINE:
                hasOrders = true;
                if (!destSet) // if a destination isn't set walk to the resource
                    ChangeDest(clickPoint);
                break;
            case Orders.UNLOAD:
                if (!destSet && carryinAmt > 0f)
                {
                    hasOrders = true;
                    ChangeDest(clickPoint);
                }
                else
                {
                    hasOrders = false;
                    currentOrders = Orders.IDLE;
                }
                break;
            case Orders.BUILD:
                hasOrders = true;
                ChangeDest(clickPoint); // Walk to the building foundation
                break;
            case Orders.IDLE: // Idle is to do nothing
                hasOrders = false;
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
                    if (isSelected)
                    {
                        hasOrders = false;
                        currentOrders = Orders.WALK;
                    }
                    break;
                case 9:
                    if (isSelected)
                    {
                        currentOrders = Orders.CHOP;
                        currentResource = hit.collider.gameObject;
                    }
                    break;
                case 10:
                    if (isSelected)
                    {
                        currentOrders = Orders.MINE;
                        currentResource = hit.collider.gameObject;
                    }
                    break;
                case 11:
                    if (isSelected)
                        currentOrders = Orders.BUILD;
                    break;
                case 12:
                    if(isSelected && carryinAmt > 0f)
                        currentOrders = Orders.UNLOAD;
                    break;
                case 13:
                    if (hit.collider.gameObject == gameObject)
                    {
                        isSelected = true;
                        hasOrders = false;
                        destSet = false;
                    }
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

    // Changes the destination of the worker
    void ChangeDest(Vector3 destination)
    {
        destSet = true;
        hasOrders = true;
        agent.SetDestination(destination);
    }
}
