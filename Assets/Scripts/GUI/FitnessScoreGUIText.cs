using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World;

public class FitnessScoreGUIText : MonoBehaviour
{
    public WorldCreator worldCreator;

    Text textComponent;
    int recentScoreStorageSize = 10;
    List<float> recentScoreStorage;

    // Start is called before the first frame update
    void Start()
    {
        textComponent = GetComponent<Text>();
        recentScoreStorage = new List<float>();
        worldCreator.OnRecreateWorlds += UpdateGUI;
    }


    private void UpdateGUI()
    {
        float bestFitness = worldCreator.BestFitnessScore;

        if (recentScoreStorage.Count == recentScoreStorageSize)
            recentScoreStorage.RemoveAt(0);

        float sum = 0f;
        for (int i = 0; i < recentScoreStorage.Count; i++)
        {
            sum += recentScoreStorage[i];
        }
        float recentChange = 0f;
        if (recentScoreStorage.Count > 0)
            recentChange = bestFitness - sum / recentScoreStorage.Count;

        recentScoreStorage.Add(bestFitness);

        if (recentChange < 0.001f) recentChange = 0f;
        textComponent.text = $"Best fitness {(int)bestFitness}\nRecent change {(int)recentChange}";
    }

    public void Reset()
    {
        textComponent = GetComponent<Text>();
        recentScoreStorage = new List<float>();
    }
}
