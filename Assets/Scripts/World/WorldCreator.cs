using DefaultNamespace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace World
{
    public class WorldCreator : MonoBehaviour
    {
        public GameObject worldPrefab;
        public SimulationObjectPrefab[] objectPrefabs;
        public TextAsset worldDefinitionFile;
        public int numberOfWorldsToCreate;
        public RabbitFitnessCalculatorOptions rabbitFitnessFunction;
        public FoxFitnessCalculatorOptions foxFitnessFunction;
        public float BestFitnessScore { get { return sortedBrainList.Count > 0 ? sortedBrainList.Keys[0] : -1; } }
        public bool RunSimulation { get; set; } = true;
        public SortedList<float, NeuralNetwork> sortedBrainList;
        public SortedList<float, NeuralNetwork> sortedFoxesBrainList;
        public Action OnRecreateWorlds;

        private List<WorldBuilder> worldOptions;
        private List<GameObject> worldGameObjects;
        private List<World> worlds;
        private Dictionary<string, GameObject> prefabs;
        private Vector2Int size;
        private float gapBetweenWorlds;
        private IFitnessCalculator rabbitFitnessCalculator;
        private IFitnessCalculator foxFitnessCalculator;

        private MultiListIterator<Animal> animalIterator;
        private MultiListIterator<Grass> grassIterator;

        private bool UpdateBehaviourAllWorlds()
        {
            SetAllObjectIterators();

            Profiler.BeginSample("rabbits");

            Parallel.ForEach(animalIterator, animal =>
            {
                animal.UpdateTurn();
            });
            animalIterator.Reset();

            Profiler.EndSample();
            Profiler.BeginSample("grass");

            Parallel.ForEach(grassIterator, grass =>
            {
                grass.UpdateTurn();
            });
            grassIterator.Reset();

            Profiler.EndSample();
            Profiler.BeginSample("worlds");

            for (int i = worlds.Count - 1; i >= 0; i--)
            {
                var alive = worlds[i].UpdateTurn();
                if (!alive)
                {
                    sortedBrainList.RemoveAt(sortedBrainList.IndexOfValue(worlds[i].BigBrain));
                    sortedBrainList.Add(rabbitFitnessCalculator.CalculateFitness(worlds[i].History), worlds[i].BigBrain);

                    sortedFoxesBrainList.RemoveAt(sortedFoxesBrainList.IndexOfValue(worlds[i].FoxBrain));
                    sortedFoxesBrainList.Add(foxFitnessCalculator.CalculateFitness(worlds[i].History), worlds[i].FoxBrain);

                    Destroy(worlds[i].gameObject);
                    animalIterator.RemoveList(worlds[i].animalList);
                    grassIterator.RemoveList(worlds[i].grassList);
                    worlds.RemoveAt(i);
                }
            }
            Profiler.EndSample();

            return worlds.Count > 0;
        }

        private void UpdateAllWorlds()
        {
            World.deltaTime = Time.deltaTime;
            var anyWorldsLeft = UpdateBehaviourAllWorlds();
            if (!anyWorldsLeft)
            {
                OnRecreateWorlds?.Invoke();
                MutateBrains();
                CreateAllWorlds();
            }
        }

        private void MutateBrains()
        {
            sortedBrainList = MutateListOfBrains(sortedBrainList);
            sortedFoxesBrainList = MutateListOfBrains(sortedFoxesBrainList);
        }

        private SortedList<float, NeuralNetwork> MutateListOfBrains(SortedList<float, NeuralNetwork> sortedBrainList)
        {
            SortedList<float, NeuralNetwork> newBrainList = new SortedList<float, NeuralNetwork>(numberOfWorldsToCreate, new ReverseDuplicateKeyComparer<float>());
            int currentBrainIndex = 0;
            IList<float> brainListFitness = sortedBrainList.Keys;
            IList<NeuralNetwork> brainListBrains = sortedBrainList.Values;
            foreach (ListMutationQuantity mutationQuantity in Settings.NeuralMutationSettings.listMutationQuantity)
            {
                for (int i = 0; i < mutationQuantity.count; i++)
                {
                    if (currentBrainIndex < brainListFitness.Count)
                    {
                        newBrainList.Add(brainListFitness[currentBrainIndex], brainListBrains[currentBrainIndex]);
                        for (int quantity = 1; quantity < mutationQuantity.quantity; quantity++)
                        {
                            newBrainList.Add(-1, NeuralNetworkMutator.Mutate(brainListBrains[currentBrainIndex]));
                        }
                    }
                    currentBrainIndex++;
                }
            }

            return newBrainList;
        }

        public void SaveBestBrainToFile(string filePath)
        {
            print("Save:  " + filePath);
            NeuralNetworkStorage.SaveToFile(filePath, sortedBrainList.Values.ToArray(), sortedBrainList.Keys.ToArray());
        }

        public void LoadBrainFromFile(string filePath)
        {
            print("Load:  " + filePath);
            DestroyAllWorlds();
            sortedBrainList = new SortedList<float, NeuralNetwork>(numberOfWorldsToCreate, new ReverseDuplicateKeyComparer<float>());
            NeuralNetwork[] brainList = NeuralNetworkStorage.ReadFromFile(filePath);
            foreach(var brain in brainList)
            {
                sortedBrainList.Add(-1f, brain);
            }
            CreateAllWorlds();
        }

        private void FixedUpdate()
        {
            if (!Settings.Player.fastTrainingMode && RunSimulation)
            {
                UpdateAllWorlds();
            }
        }

        private void Update()
        {
            if (Settings.Player.fastTrainingMode && RunSimulation)
            {
                UpdateAllWorlds();
            }
            if(RunSimulation)
            {
                SetMoveableObjectIterators();
                foreach (Animal animal in  animalIterator)
                {
                    animal.UpdatePosition();
                }
                animalIterator.Reset();
            }
        }

        private Vector2Int CalculateSize(int numOfWorlds)
        {
            for (int d = (int)Mathf.Sqrt(numOfWorlds); d > 1; d--)
            {
                if (d * (numOfWorlds / d) == numOfWorlds)
                {
                    return new Vector2Int(numOfWorlds / d, d);
                }
            }
            return new Vector2Int(numOfWorlds, 1);
        }

        ///////////////////////////////////////////////////
        /// Init world
        private void Start()
        {
            gapBetweenWorlds = 1;
            sortedBrainList = new SortedList<float, NeuralNetwork>(numberOfWorldsToCreate, new ReverseDuplicateKeyComparer<float>());
            sortedFoxesBrainList = new SortedList<float, NeuralNetwork>(numberOfWorldsToCreate, new ReverseDuplicateKeyComparer<float>());
            rabbitFitnessCalculator = rabbitFitnessFunction.GetCalculator();
            foxFitnessCalculator = foxFitnessFunction.GetCalculator();
            // Create field objects
            InitResources();
            // Fill List<WorldBuilder> worldOptions from input file (TextAsset worldDefinitionFile)
            ParseInputFile();
            // Create worlds based on int numberOfWorldsToCreate
            CreateAllWorlds();
        }

        public void ResetWorlds()
        {
            DestroyAllWorlds();
            sortedBrainList = new SortedList<float, NeuralNetwork>(numberOfWorldsToCreate, new ReverseDuplicateKeyComparer<float>());
            sortedFoxesBrainList = new SortedList<float, NeuralNetwork>(numberOfWorldsToCreate, new ReverseDuplicateKeyComparer<float>());
            CreateAllWorlds();
        }

        public void DestroyAllWorlds()
        {
            for (int i = worlds.Count - 1; i >= 0; i--)
            {
                Destroy(worlds[i].gameObject);
                worlds.RemoveAt(i);
            }
        }

        private void CreateAllWorlds()
        {
            
            float timeStart = Time.realtimeSinceStartup;
            Vector3 offset = Vector3.zero;  // offset to bottom-left corner
            int worldsOnX = 0;

            for(int i = sortedBrainList.Count; i<numberOfWorldsToCreate; i++)
            {
                sortedBrainList.Add(-1f, new NeuralNetwork(Settings.Player.neuralNetworkLayers));
            }

            for (int i = sortedFoxesBrainList.Count; i < numberOfWorldsToCreate; i++)
            {
                sortedFoxesBrainList.Add(-1f, new NeuralNetwork(Settings.Player.neuralNetworkLayers));
            }

            WorldBuilder builder = worldOptions[UnityEngine.Random.Range(0, worldOptions.Count)];
            builder.StartNewBuild();
            for (int i = 0; i < numberOfWorldsToCreate; i++)
            {
                GameObject world = CreateRandomWorld(builder, ref offset, ref worldsOnX, i);
                worldGameObjects.Add(world);
                worlds.Add(world.GetComponent<World>());
            }
           
            Debug.Log("CreateAllWorlds time: " + (Time.realtimeSinceStartup - timeStart));
        }

        private void SetAllObjectIterators()
        {
            animalIterator = new MultiListIterator<Animal>();
            grassIterator = new MultiListIterator<Grass>();
            foreach(World world in this.worlds)
            {
                animalIterator.AddList(world.animalList);
                grassIterator.AddList(world.grassList);

            }
        }

        private void SetMoveableObjectIterators()
        {
            animalIterator = new MultiListIterator<Animal>();
            foreach (World world in this.worlds)
            {
                animalIterator.AddList(world.animalList.ConvertAll(r => r as Animal));
            }
        }

        private GameObject CreateRandomWorld(WorldBuilder builder, ref Vector3 offset, ref int worldsOnX, int index)
        {
            GameObject world = Instantiate(worldPrefab, this.transform);
            world.name = "World_" + index;
            world.GetComponent<World>().Render = Settings.Player.renderOptions == RenderOptions.Reduced 
                || Settings.Player.renderOptions == RenderOptions.Full 
                || index == 0 && Settings.Player.renderOptions == RenderOptions.OnlyBest;
            world.GetComponent<World>().BigBrain = sortedBrainList.Values[index];
            world.GetComponent<World>().FoxBrain = sortedFoxesBrainList.Values[index];
            var lastSize = builder.CreateWorld(world, offset, null, prefabs);   // Apply setup from file to given world
            worldsOnX++;
            offset.x += gapBetweenWorlds + lastSize.x;
            if (worldsOnX >= size.x)    // Start new row
            {
                offset.x = 0;
                offset.z += gapBetweenWorlds + lastSize.y;
                worldsOnX = 0;
            }
            return world;
        }

        private void ParseInputFile()
        {
            foreach (var line in worldDefinitionFile.text.Split('\n'))
            {
                try
                {
                    worldOptions.Add(new WorldBuilder(line));
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                    Debug.LogError(e.StackTrace);
                }
            }
        }

        private void InitResources()
        {
            worldGameObjects = new List<GameObject>();
            worlds = new List<World>();
            prefabs = new Dictionary<string, GameObject>();
            worldOptions = new List<WorldBuilder>();

            foreach (var prefabInfo in objectPrefabs)
            {
                prefabs.Add(prefabInfo.name, prefabInfo.prefab);
            }

            size = CalculateSize(numberOfWorldsToCreate);
        }
    }

    [System.Serializable]
    public struct SimulationObjectPrefab
    {
        public string name;
        public GameObject prefab;
    }
}