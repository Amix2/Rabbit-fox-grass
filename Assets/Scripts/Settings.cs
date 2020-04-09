using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{

    public PlayerSettings player;
    public static PlayerSettings Player
    {
        get { return instance.player; }
    }

    public WorldSettings world;
    public static WorldSettings World
    {
        get { return instance.world; }
    }

    public NeuralNetworkSettings neuralNetwork;
    public static NeuralNetworkSettings NeuralNetwork
    {
        get { return instance.neuralNetwork; }
    }


    private static Settings instance;

    private void Awake()
    {
        instance = this;
        instance.world.simulationDeltaTime = Time.fixedDeltaTime;
    }
}

[System.Serializable]
public class PlayerSettings
{
    public float mouseSensitivity = 150f;
    public float cameraMoveSensitivity = 0.001f;
    public float cameraScrollSensitivity = 0.1f;
    public float cameraRotateSensitivity = 500f;
    public bool fastTrainingMode = false;
    public RenderOptions renderOptions = RenderOptions.Full;
    public int[] neuralNetworkLayers = new[] { 2, 10, 10, 2 };
}

[System.Serializable]
public class WorldSettings
{
    public float animalViewRange = 10f;
    public string[] allowedObjectNames;
    public float foodInGrass = 0.5f;
    public float grassGrowthRate = 0.5f;   // per sec
    public float foodInRabbits = 1f;
    public float rabbitEatingDistance = 1.3f;
    public float rabbitMaxVelocity = 2f;   // per sec
    public float rabbitHungerRate = 0.5f;   // per sec
    public float rabbitEatingSpeed = 1f;   // per sec
    public float simulationDeltaTime;
}

[System.Serializable]
public class NeuralNetworkSettings
{
    public int rabbitFistLayerSize = 11;
    public int[] rabbitNetworkHiddenLayers;
    public int outputLayerSize = 2;
}

public enum RenderOptions
{
    Full,
    Reduced,
    None
}
