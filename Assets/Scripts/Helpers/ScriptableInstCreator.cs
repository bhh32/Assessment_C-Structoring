using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableInstCreator : MonoBehaviour 
{
    public ListOfResources resourceLists;
    void Awake()
    {
        resourceLists = ScriptableObject.CreateInstance("ListOfResources") as ListOfResources;
    }
}
