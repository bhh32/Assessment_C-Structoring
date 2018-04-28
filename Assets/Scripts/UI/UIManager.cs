using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public delegate void UpdateResourceTotals();
    public UpdateResourceTotals OnResourceUpdate;
        
    
    GameObject[] storageFacs;

    float totalWood;
    float totalGold;

    float goldMax;
    float woodMax;

    public Text currentGold;
    public Text maxGold;
    public Text currentWood;
    public Text maxWood;

    private void Awake()
    {
        OnResourceUpdate += UpdateResources;
        currentGold.text = 0f.ToString();
        maxGold.text = 0f.ToString();
        currentWood.text = 0f.ToString();
        maxWood.text = 0f.ToString();
    }

    void UpdateResources()
    {
        float tempGold = 0f;
        float tempWood = 0f;

        storageFacs = GameObject.FindGameObjectsWithTag("Storage");

        foreach (GameObject store in storageFacs)
        {
            var tempStore = store.GetComponent<Storage>();
            tempGold += tempStore.CurrentGold;
            tempWood += tempStore.CurrentWood;
        }

        totalGold += tempGold;
        totalWood += tempWood;

        SetUI(totalGold, totalWood);
    }

    void SetUI(float gold, float wood)
    {
        storageFacs = GameObject.FindGameObjectsWithTag("Storage");
        float tempGold = 0f;
        float tempWood = 0f;
        for (int i = 0; i < storageFacs.Length; ++i)
        {
            var resources = storageFacs[i].GetComponent<Storage>();
            tempGold += resources.GoldStorageMax;
            tempWood += resources.WoodStorageMax;
        }
        goldMax = tempGold;
        woodMax = tempWood;

        maxGold.text = goldMax.ToString();
        maxWood.text = woodMax.ToString();

        currentGold.text = gold.ToString();
        currentWood.text = wood.ToString();
    }
}
