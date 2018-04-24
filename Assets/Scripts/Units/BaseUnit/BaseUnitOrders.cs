using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BaseUnitOrders : MonoBehaviour, UnitOrders
{
    GameObject previousResource = null;

    public void Move(NavMeshAgent agent)
    {
        Ray newPos = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(newPos, out hit, 200f))
        { agent.SetDestination(hit.point); }
    }

    public void StartingMove(NavMeshAgent agent)
    {
        Vector3 startPos = new Vector3(Random.Range(1f, 3f), 0f, Random.Range(1f, 3f));

        agent.SetDestination(transform.position + startPos);
    }

    public void Attack(NavMeshAgent agent, GameObject defender)
    {        
    }

    public void Explore()
    {
    }

    public void ChopWood(NavMeshAgent agent, GameObject trees)
    {
        float dist = Vector3.Distance(agent.transform.position, trees.transform.position);
        var harvestOrder = agent.GetComponent<WorkerOrders>();
        var resource = trees.GetComponent<BaseResource>();
        if (harvestOrder.CurrentCarryingAmt < harvestOrder.MaxCarryingAmt)
        {
            if (dist > .5f)
            {
                Move(agent);
                StartCoroutine(MineralMove(dist, agent, trees));
            }

            resource.OnResourceMined();
        }
    }

    public void Mine(NavMeshAgent agent, GameObject mineral)
    {
        float dist = Vector3.Distance(agent.transform.position, mineral.transform.position);
        var miningOrder = agent.GetComponent<WorkerOrders>();
        if (miningOrder.CurrentOrders != Orders.MINE)
            miningOrder.CurrentOrders = Orders.MINE;

        var resource = mineral.GetComponent<BaseResource>();
        if (miningOrder.CurrentCarryingAmt < miningOrder.MaxCarryingAmt)
        {
            if (dist > .5f)
            {
                Move(agent);
                StartCoroutine(MineralMove(dist, agent, mineral));
            }

            resource.OnResourceMined();
        }            
    }

    public void Unload(NavMeshAgent agent, GameObject storageFac)
    {
        float dist = Vector3.Distance(agent.transform.position, storageFac.transform.position);
        agent.SetDestination(storageFac.transform.position);
        StartCoroutine(StorageMove(dist, agent, storageFac));
    }

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

    public GameObject FindClosestMine(GameObject previousMine, GameObject[] mines)
    {
        float dist = 0f;
        GameObject returnObj = null;

        foreach (GameObject mine in mines)
        {
            float currentDist = Vector3.Distance(previousMine.transform.position, mine.transform.position);

            if (mine == mines[0])
            {
                returnObj = mine;
                dist = currentDist;
            }

            if (currentDist < dist)
            {
                returnObj = mine;
                dist = currentDist;
            }
        }

        return returnObj;
    }

    IEnumerator MineralMove(float dist, NavMeshAgent agent, GameObject mineral)
    {
        previousResource = mineral;

        if (agent.destination != mineral.transform.position)
            agent.SetDestination(mineral.transform.position);

        while (dist > .5f)
        {
            dist = Vector3.Distance(agent.transform.position, mineral.transform.position);
            yield return null;
        }
    }

    IEnumerator StorageMove(float dist, NavMeshAgent agent, GameObject facility)
    {
        Debug.Log(previousResource.tag);
        while (dist > 1.25f)
        {
            dist = Vector3.Distance(agent.transform.position, facility.transform.position);
            yield return null;
        }
        agent.GetComponent<WorkerOrders>().CurrentCarryingAmt = 0f;
        //TODO: NEW COROUTINE TO UNLOAD OVER TIME

//        if (previousResource.GetComponent<BaseResource>().MaxAmt == 0f)
//        {
//            var mines = GameObject.FindGameObjectsWithTag("Minable");
//
//            previousResource = FindClosestMine(previousResource, mines);
//
//            if (Vector3.Distance(agent.transform.position, previousResource.transform.position) < 10f)
//                Mine(agent, previousResource);
//        }
//        else
//        {
            Mine(agent, previousResource);
//        }

    }

    public enum Orders
    {
        MOVE, ATTACK, EXPLORE, CHOP, MINE, UNLOAD, EMPTY, ERROR
    }
}
