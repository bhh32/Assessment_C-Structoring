using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    
    GameObject[] storageFacs;

    public Text currentGold;
    public Text maxGold;
    public Text currentWood;
    public Text maxWood;

    private void Awake()
    {
        currentGold.text = 0f.ToString();
        maxGold.text = 0f.ToString();
        currentWood.text = 0f.ToString();
        maxWood.text = 0f.ToString();
    }
}
