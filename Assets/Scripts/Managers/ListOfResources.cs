using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListOfResources : ScriptableObject
{
    public delegate void ResourceLists(GameObject Resources);
    public ResourceLists OnAddResource;
    public ResourceLists OnRemoveResource;

    List<GameObject> mines;
    List<GameObject> trees;

    void Awake()
    {
        trees = new List<GameObject>();
        mines = new List<GameObject>();
        OnRemoveResource += RemoveResource;
        OnAddResource += AddResource;
    }

    void AddResource(GameObject newResource)
    {
        switch (newResource.tag)
        {
            case "Minable":
                if(!mines.Contains(newResource))
                    mines.Add(newResource);
                break;
            case "Choppable":
                if(!trees.Contains(newResource))
                    trees.Add(newResource);
                break;
            default:
                Debug.LogWarning("This Wasn't A Resource!");
                break;
        }

        Debug.Log(string.Format("Resource {0} was added", newResource.tag));
    }

    void RemoveResource(GameObject removeResource)
    {
        switch (removeResource.tag)
        {
            case "Minable":
                for(int i = 0; i < mines.Count; ++i)
                {
                    if (mines[i].Equals(removeResource))
                        mines.RemoveAt(i);
                }
                break;
            case "Choppable":
                for(int i = 0; i < mines.Count; ++i)
                {
                    if (trees[i].Equals(removeResource))
                        trees.RemoveAt(i);
                }
                break;
            default:
                Debug.LogWarning("This Wasn't A Resource!");
                break;
        }

        Debug.Log(string.Format("Resource {0} was removed", removeResource.tag));
    }
}
