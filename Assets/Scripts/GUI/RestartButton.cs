using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using World;

public class RestartButton : MonoBehaviour
{
    public WorldCreator worldCreator;
    public FitnessScoreGUIText fitnessScore;

    public void OnClickReset()
    {
        worldCreator.ResetWorlds();
        fitnessScore.Reset();
    }
}
