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
    private GameObject foundation;
    public GameObject Foundation
    { get { return foundation; } }
	
	//public GameObject Foundation { get; private set; } // ???

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
            other.GetComponent<WorkerOrders>().Build(agent, gameObject, CurrentBuilding);
        }
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
