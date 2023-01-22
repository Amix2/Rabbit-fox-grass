using DefaultNamespace;
using Newtonsoft.Json;
using System.IO;
using UnityEditor;
using UnityEngine;
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

    public RabbitSettings rabbitSettings;

    public static RabbitSettings Rabbit
    {
        get { return instance.rabbitSettings; }
    }

    public FoxSettings foxSettings;

    public static FoxSettings Fox
    {
        get { return instance.foxSettings; }
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
#if UNITY_EDITOR
#else
        SettingsSerialized settings = SettingsSerialized.ReadFromFile();
        instance.player = settings.Player;
        instance.world = settings.World;
        instance.rabbitSettings = settings.RabbitSettings;
        instance.foxSettings = settings.FoxSettings;
        instance.neuralMutation = settings.NeuralMutation;
#endif
        instance.world.simulationDeltaTime = Time.fixedDeltaTime*10;

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
    public bool collectHistory = true;
    public float simulationDeltaTime;
    public float maxAnimalLifetime;
    public float multipliedAnimalSpawnRadius = 1.0f;

    [Newtonsoft.Json.JsonIgnore]
    public TextAsset worldDefinitionFile;

    public int numberOfWorldsToCreate;
}

[System.Serializable]
public class RabbitSettings
{
    public int[] brainParams;
    public float foodInRabbits = 1f;
    public float rabbitEatingDistance = 1.3f;
    public float rabbitMaxVelocity = 2f;   // per sec
    public float rabbitHungerRate = 0.5f;   // per sec
    public float rabbitEatingSpeed = 1f;   // per sec
    public float rabbitMultiplicationChance = 0.1f;
    public float healthDropAfterMultiplied = 0.5f;
    public int maxAnimalsInScene;
}

[System.Serializable]
public class FoxSettings
{
    public int[] brainParams;
    public float foxEatingDistance = 1.3f;
    public float foxMaxVelocity = 2f;   // per sec
    public float foxHungerRate = 0.5f;   // per sec
    public float foxEatingSpeed = 1f;   // per sec
    public float foxMultiplicationChance = 0.1f;
    public float healthDropAfterMultiplied = 0.5f;
    public int maxAnimalsInScene;
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

[System.Serializable]
public class SettingsSerialized
{
    private PlayerSettings player;
    private WorldSettings world;
    private RabbitSettings rabbitSettings;
    private FoxSettings foxSettings;
    private NeuralMutationSettings neuralMutation;
    private string worldFileName;

    [Newtonsoft.Json.JsonIgnore]
    public static string Path => Application.streamingAssetsPath + "/config.json";

    public string WorldFileName { get => worldFileName; set => worldFileName = value; }
    public PlayerSettings Player { get => player; set => player = value; }
    public WorldSettings World { get => world; set => world = value; }
    public RabbitSettings RabbitSettings { get => rabbitSettings; set => rabbitSettings = value; }
    public FoxSettings FoxSettings { get => foxSettings; set => foxSettings = value; }
    public NeuralMutationSettings NeuralMutation { get => neuralMutation; set => neuralMutation = value; }

    public SettingsSerialized(Settings settings)
    {
#if UNITY_EDITOR
        player = settings.player;
        world = settings.world;
        rabbitSettings = settings.rabbitSettings;
        foxSettings = settings.foxSettings;
        neuralMutation = settings.neuralMutation;
        worldFileName = AssetDatabase.GetAssetPath(world.worldDefinitionFile).Replace("Assets/Resources/", "").Replace(".txt", "");
#endif
    }

    public SettingsSerialized()
    {
    }

    public static SettingsSerialized ReadFromFile()
    {
        string jsonText = File.ReadAllText(Path);
        SettingsSerialized settings = JsonConvert.DeserializeObject<SettingsSerialized>(jsonText);
        settings.world.worldDefinitionFile = Resources.Load<TextAsset>(settings.worldFileName);
        return settings;
    }
}