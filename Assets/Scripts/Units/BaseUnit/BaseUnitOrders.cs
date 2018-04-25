using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BaseUnitOrders : MonoBehaviour, UnitOrders
{
    // Keeps track of the previous resource visited
    GameObject previousResource = null;
    [SerializeField] float stoppingDistance;

    public void Move(NavMeshAgent agent)
    {
        if (agent.isStopped)
            agent.isStopped = false;
        Ray newPos = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(newPos, out hit, 200f))
        { agent.SetDestination(hit.point); Debug.Log(hit.collider.name); }
    }

    // Unit Moves After Being Spawned
    public void StartingMove(NavMeshAgent agent)
    {
        Vector3 startPos = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-1f, 1f));

        agent.SetDestination(transform.position + startPos);
    }

    public void Build(NavMeshAgent agent, GameObject foundation, GameObject building)
    {
        MoveToBuild(agent, foundation);

        GameObject newBuilding = null;
        newBuilding = Instantiate(building, foundation.transform.position, Quaternion.identity) as GameObject;
        newBuilding.transform.parent = GameObject.FindWithTag("BuildingContainer").transform;

        agent.GetComponent<WorkerOrders>().CurrentOrders = Orders.EMPTY;
    }

    public void MoveToBuild(NavMeshAgent agent, GameObject foundation)
    {
        StartCoroutine(MoveToFoundation(agent, foundation));
    }

    IEnumerator MoveToFoundation(NavMeshAgent agent, GameObject foundation)
    {
        float dist = Vector3.Distance(agent.transform.position, foundation.transform.position);
        agent.SetDestination(foundation.transform.position);

        while (dist > stoppingDistance)
        {
            dist = Vector3.Distance(agent.transform.position, foundation.transform.position);

            yield return null;
        }

        if (dist < stoppingDistance)
        {
            agent.isStopped = true;
        }

    }

    public void Attack(NavMeshAgent agent, GameObject defender)
    {        
    }

    public void Explore()
    {
    }

    // Sends the player to the selected mine/previous mine
    public void TakeResource(NavMeshAgent agent, GameObject resource)
    {
        float dist = Vector3.Distance(agent.transform.position, resource.transform.position);
        var takeOrder = agent.GetComponent<WorkerOrders>();

        var resourceProperties = resource.GetComponent<BaseResource>();

        if (takeOrder.CurrentCarryingAmt < takeOrder.MaxCarryingAmt)
        {
            if (dist > .5f)
            {
                Move(agent);
                StartCoroutine(ResourceMove(dist, agent, resource));
            }
        }            
    }

    // Moves the worker to the closest storage facility to unload current carrying capacity
    public void Unload(NavMeshAgent agent, GameObject storageFac)
    {
        float dist = Vector3.Distance(agent.transform.position, storageFac.transform.position);
        agent.SetDestination(storageFac.transform.position);
        StartCoroutine(StorageMove(dist, agent, storageFac));
    }

    // Finds the closest storage unit to the worker
    public GameObject FindClosestStorage(NavMeshAgent agent, GameObject[] storageFacilities)
    {
        float dist = 0f;
        GameObject returnObj = null;

        foreach (GameObject fac in storageFacilities)
        {
            float currentDist = Vector3.Distance(agent.transform.position, fac.transform.position);

            if (fac == storageFacilities[0])
            {
                returnObj = fac;
                dist = currentDist;
            }

            if (currentDist < dist)
            {
                returnObj = fac;
                dist = currentDist;
            }
        }

        return returnObj;
    }

    // Finds the closest mine to the current mine the worker is working in
    public GameObject FindClosestResource(GameObject previousResource, GameObject[] resources)
    {
        float dist = Mathf.Infinity;
        GameObject returnObj = null;

        foreach (GameObject resource in resources)
        {
            float currentDist = Vector3.Distance(previousResource.transform.position, resource.transform.position);

            if (currentDist < dist && resource.GetComponent<BaseResource>().MaxAmt > 0f)
            {
                returnObj = resource;
                dist = currentDist;
            }
        }

        return returnObj;
    }

    IEnumerator ResourceMove(float dist, NavMeshAgent agent, GameObject resource)
    {
        if(previousResource != resource)
            previousResource = resource;

        if (agent.destination != resource.transform.position)
            agent.SetDestination(resource.transform.position);

        while (dist > .5f)
        {
            dist = Vector3.Distance(agent.transform.position, resource.transform.position);
            yield return null;
        }
    }

    IEnumerator StorageMove(float dist, NavMeshAgent agent, GameObject facility)
    {
        // Populate the resource array with the closest same type resource
        GameObject[] resource = null;
        if (previousResource != null)
        {
            switch (previousResource.tag)
            {
                case "Minable":
                    resource = GameObject.FindGameObjectsWithTag("Minable");
                    break;
                case "Choppable":
                    resource = GameObject.FindGameObjectsWithTag("Choppable");
                    break;
            }
       
            // Ensure the order is TAKE
            WorkerOrders orders = agent.GetComponent<WorkerOrders>();
            if (orders.CurrentOrders != Orders.TAKE)
                orders.CurrentOrders = Orders.TAKE;

            while (dist > 1.25f)
            {
                dist = Vector3.Distance(agent.transform.position, facility.transform.position);
                yield return null;
            }

            //TODO: NEW COROUTINE TO UNLOAD OVER TIME
            agent.GetComponent<WorkerOrders>().CurrentCarryingAmt = 0f;

            if (previousResource.GetComponent<BaseResource>().MaxAmt > 0f)
            {               
                TakeResource(agent, previousResource);
            }
            else
            {
                // Set the previous resource visited
                previousResource = FindClosestResource(previousResource, resource);
                float dist2 = Vector3.Distance(agent.transform.position, previousResource.transform.position);
                if (dist2 < 15f)
                    TakeResource(agent, previousResource);
                else
                    agent.GetComponent<WorkerOrders>().CurrentOrders = Orders.MOVE;
            }
        }
    }

    public enum Orders
    {
        MOVE, BUILD, ATTACK, EXPLORE, TAKE, UNLOAD, EMPTY, ERROR
    }
}
