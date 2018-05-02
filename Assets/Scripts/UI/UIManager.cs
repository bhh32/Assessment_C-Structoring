using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Delegate to be called when resources need to be updated
    public delegate void UpdateTotalResources();
    public UpdateTotalResources OnResourceChange;
    
    GameObject[] storageFacs; // Holds all of the storage facilities

    float maxWood; // Max wood all of the storage facilites can hold
    float maxGold; // Max gold all of the storage facilites can hold
    float totalWood; // Total wood currently stored in all storage facilities
    float totalGold; // Total gold currently stored in all storage facilities

    [SerializeField] Text currentGoldText; // Text displaying all of the currently stored gold
    [SerializeField] Text maxGoldText; // Text displaying the max gold than can be stored
    [SerializeField] Text currentWoodText; // Text displaying all of the currently stored wood
    [SerializeField] Text maxWoodText; // Text displaying the max wood that can be stored


    private void Awake()
    {
        OnResourceChange += UpdateResources;
        storageFacs = GameObject.FindGameObjectsWithTag("Storage");
        currentGoldText.text = 0f.ToString();
        maxGoldText.text = TotalGold().ToString();
        currentWoodText.text = 0f.ToString();
        maxWoodText.text = TotalWood().ToString();
    }

    float TotalGold()
    {
        maxGold = 0;

        foreach(GameObject facs in storageFacs)
        {
            maxGold += facs.GetComponent<Storage>().maxGoldCapacity;
        }

        return maxGold;
    }

    float TotalWood()
    {
        maxWood = 0;

        foreach (GameObject facs in storageFacs)
            maxWood += facs.GetComponent<Storage>().maxWoodCapacity;

        return maxWood;
    }

    void UpdateResources()
    {
        totalGold = 0;
        totalWood = 0;

        storageFacs = GameObject.FindGameObjectsWithTag("Storage");


        foreach(GameObject facs in storageFacs)
        {
            var script = facs.GetComponent<Storage>();

            totalWood += script.currentWoodCapacity;
            totalGold += script.currentGoldCapacity;
        }

        currentGoldText.text = totalGold.ToString();
        currentWoodText.text = totalWood.ToString();

        maxGoldText.text = TotalGold().ToString();
        maxWoodText.text = TotalWood().ToString();
    }
}
