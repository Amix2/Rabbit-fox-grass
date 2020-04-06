using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public Player player;

    public static Player Player
    {
        get { return instance.player; }
    }


    private static Settings instance;

    private void Awake()
    {
        instance = this;
    }
}

[System.Serializable]
public class Player
{
    public float mouseSensitivity = 150f;
    public float cameraMoveSensitivity = 0.001f;
    public float cameraScrollSensitivity = 0.1f;
    public float cameraRotateSensitivity = 500f;
    public float animalViewRange = 10f;
    public bool fastTrainingMode = false;
    public string[] allowedObjectNames;
    public int[] neuralNetworkLayers = new[] {2, 10, 10, 2};
}