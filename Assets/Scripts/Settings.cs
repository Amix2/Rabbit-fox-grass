using Boo.Lang;
using DefaultNamespace;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;

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


    public NeuralMutationSettings neuralMutation;
    public static NeuralMutationSettings NeuralMutationSettings
    {
        get { return instance.neuralMutation; }
    }


    private static Settings instance;

    private void Awake()
    {
        instance = this;
        instance.world.simulationDeltaTime = Time.fixedDeltaTime;
        Profiler.enabled = false;
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
    public double[] neuralNetworkWeightsRange = new[] { -4.0, 4.0 };
    public double[] neuralNetworkBiasRange = new[] { -4.0, 4.0 };
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
    public bool collectHistory = true;
    public float simulationDeltaTime;
    public float maxAnimalLifetime;
}

[System.Serializable]
public class NeuralNetworkSettings
{
    //public int rabbitFistLayerSize = 11;
   // public int[] rabbitNetworkHiddenLayers;
   // public int outputLayerSize = 2;
}

[System.Serializable]
public class NeuralMutationSettings
{
    public double mutationProbability = 0.01;
    [FormerlySerializedAs("mutationStrategy")] public MutationStrategyEnum mutationStrategyEnum = MutationStrategyEnum.Random;
    [SerializeField] public ListMutationQuantity[] listMutationQuantity;
}

public enum RenderOptions
{
    Full,
    Reduced,
    OnlyBest,
    None
}

[System.Serializable]
public class ListMutationQuantity
{
    public int count;
    public int quantity;
}
