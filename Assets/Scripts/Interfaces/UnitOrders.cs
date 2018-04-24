using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public interface UnitOrders
{
    void Move(NavMeshAgent agent);
    void Attack(NavMeshAgent agent, GameObject defender);
    void Explore();
    void TakeResource(NavMeshAgent agent, GameObject mineral);
    GameObject FindClosestStorage(NavMeshAgent agent, GameObject[] storageFacilities);
    void Unload(NavMeshAgent agent, GameObject storageFac);
}
