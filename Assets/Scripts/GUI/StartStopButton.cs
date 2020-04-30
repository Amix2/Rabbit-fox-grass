using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World;

public class StartStopButton : MonoBehaviour
{
    public WorldCreator worldCreator;
    private Text text;
    
    public void OnClickStartStop()
    {
        worldCreator.RunSimulation = !worldCreator.RunSimulation;
        if(worldCreator.RunSimulation)
        {
            text.text = "Stop";
        }
        else
        {
            text.text = "Start";
        }
    }

    private void Start()
    {
        text = GetComponentInChildren<Text>();
        if (worldCreator.RunSimulation)
        {
            text.text = "Stop";
        }
        else
        {
            text.text = "Start";
        }
    }
}
