using UnityEngine;
using UnityEngine.AI;

public class Buildings : MonoBehaviour 
{
    public delegate void SetNewBuilding(GameObject newBuilding);
    public SetNewBuilding OnSetBuilding;

    [SerializeField] bool canBuild; // Get Rid of SerializeField after debug
    public bool CanBuild
    {
        get{ return canBuild; }
    }
    NavMeshAgent agent;
    [SerializeField] GameObject foundation;
    public GameObject Foundation
    {
        get { return foundation; }
        private set { foundation = value; }
    }

    [SerializeField] GameObject currentBuilding;
    public GameObject CurrentBuilding
    {
        get { return currentBuilding; }
    }

	// Use this for initialization
	void Awake() 
    {
        OnSetBuilding += SetBuilding;
	}

    void Start()
    {
        canBuild = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Worker") && CanBuild && 
            other.GetComponent<WorkerOrders>().CurrentOrders == BaseUnitOrders.Orders.BUILD)
        {
            agent = other.GetComponent<NavMeshAgent>();
            agent.isStopped = true;
            canBuild = false;
            BuildAction(agent, CurrentBuilding);
        }
    }

    void BuildAction(NavMeshAgent agent, GameObject building)
    {
        GameObject newBuilding = null;
        newBuilding = Instantiate(building, transform.position, Quaternion.identity) as GameObject;
        newBuilding.transform.parent = GameObject.FindWithTag("BuildingContainer").transform;

        agent.GetComponent<WorkerOrders>().CurrentOrders = BaseUnitOrders.Orders.MOVE;
    }

    void SetBuilding(GameObject building)
    {
        currentBuilding = building;
    }

    void OnMouseDown()
    {
        foundation = gameObject;
    }
}
