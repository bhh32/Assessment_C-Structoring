using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buttons : MonoBehaviour
{
    public void Quit()
    {
        Application.Quit();
    }

    public void Pause()
    {
        if (Time.timeScale == 1f)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
    }
}
