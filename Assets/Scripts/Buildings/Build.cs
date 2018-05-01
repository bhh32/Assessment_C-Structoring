using UnityEngine;

public class Build : MonoBehaviour
{
    [SerializeField] GameObject building;

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Worker"))
        {
            var worker = other.GetComponent<Worker>();

            if(worker.currentOrders == Worker.Orders.BUILD)
            {
                GameObject newBuilding = Instantiate(building, transform.position, Quaternion.identity) as GameObject;
                newBuilding.transform.parent = GameObject.Find("Buildings").transform;

                Collider[] colliders = gameObject.GetComponents<Collider>();
                foreach (Collider collider in colliders)
                    collider.enabled = false;
            }
        }
    }
}
