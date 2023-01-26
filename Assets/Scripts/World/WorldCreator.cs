using DefaultNamespace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace World
{
    public class WorldCreator : MonoBehaviour
    {
        public static Dictionary<string, GameObject> prefabs;
        public GameObject worldPrefab;
        public SimulationObjectPrefab[] objectPrefabs;
        public RabbitFitnessCalculatorOptions rabbitFitnessFunction;
        public FoxFitnessCalculatorOptions foxFitnessFunction;
        public float BestFitnessScore { get { return sortedRabbitBrainList.Count > 0 ? sortedRabbitBrainList.Keys[0] : -1; } }
        public bool RunSimulation { get; set; } = true;
        public SortedList<float, IAnimalBrain> sortedRabbitBrainList;
        public SortedList<float, IAnimalBrain> sortedFoxesBrainList;
        public Action OnRecreateWorlds;

        private List<WorldBuilder> worldOptions;
        private List<GameObject> worldGameObjects;
        public List<World> worlds;

        private Vector2Int size;
        private float gapBetweenWorlds;
        private IFitnessCalculator rabbitFitnessCalculator;
        private IFitnessCalculator foxFitnessCalculator;

        private MultiListIterator<Animal> animalIterator;
        private MultiListIterator<Grass> grassIterator;

        private int iterationNumber = 0;



        private bool UpdateBehaviourAllWorlds()
        {
            SetAllObjectIterators();

            Profiler.BeginSample("rabbits");

            //Parallel.ForEach(animalIterator, animal =>
            //{
            //    animal.UpdateTurn();
            //});

            foreach(Animal animal in animalIterator)
            {
                animal.UpdateTurn();
            }
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
                    sortedRabbitBrainList.RemoveAt(sortedRabbitBrainList.IndexOfValue(worlds[i].RabbitBrain));
                    sortedRabbitBrainList.Add(rabbitFitnessCalculator.CalculateFitness(worlds[i].History), worlds[i].RabbitBrain);

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
                iterationNumber++;
                if(iterationNumber%100 == 0)
                {
                    string brainPath = Application.streamingAssetsPath + @"\brainTemp\" ;
                    if (!Directory.Exists(brainPath))
                    {
                        Directory.CreateDirectory(brainPath);
                    }
                    string id = DateTime.Now.ToLongTimeString().Replace(":", "-");
                    SaveRabbitBrainToFile(brainPath + "rabbit_brain_"+id+"_" + iterationNumber + ".txt");
                    SaveFoxBrainToFile(brainPath + "fox_brain_" + id + "_" + iterationNumber + ".txt");
                }
                MutateBrains();
                DestroyAllWorlds();
                CreateAllWorlds();
            }
        }

        private void MutateBrains()
        {
            sortedRabbitBrainList = MutateListOfBrains(sortedRabbitBrainList);
            sortedFoxesBrainList = MutateListOfBrains(sortedFoxesBrainList);
        }

        private SortedList<float, IAnimalBrain> MutateListOfBrains(SortedList<float, IAnimalBrain> sortedBrainList)
        {
            SortedList<float, IAnimalBrain> newBrainList = new SortedList<float, IAnimalBrain>(Settings.World.numberOfWorldsToCreate, new ReverseDuplicateKeyComparer<float>());
            int currentBrainIndex = 0;
            IList<float> brainListFitness = sortedBrainList.Keys;
            IList<IAnimalBrain> brainListBrains = sortedBrainList.Values;
            foreach (ListMutationQuantity mutationQuantity in Settings.NeuralMutationSettings.listMutationQuantity)
            {
                for (int i = 0; i < mutationQuantity.count; i++)
                {
                    if (currentBrainIndex < brainListFitness.Count)
                    {
                        newBrainList.Add(brainListFitness[currentBrainIndex], brainListBrains[currentBrainIndex]);
                        for (int quantity = 1; quantity < mutationQuantity.quantity; quantity++)
                        {
                            Profiler.BeginSample("Mutate 1 Brain");
                            newBrainList.Add(-1, BrainMutator.Mutate(brainListBrains[currentBrainIndex]));
                            Profiler.EndSample();
                        }
                    }
                    currentBrainIndex++;
                }
            }

            return newBrainList;
        }

        public void SaveRabbitBrainToFile(string filePath)
        {
            print("Save rabbit:  " + filePath);
            NeuralNetworkStorage.SaveToFile(filePath, sortedRabbitBrainList.Values.ToArray(), sortedRabbitBrainList.Keys.ToArray());
        }

        public void LoadRabbitBrainFromFile(string filePath)
        {
            print("Load rabbit:  " + filePath);
            DestroyAllWorlds();
            sortedRabbitBrainList = new SortedList<float, IAnimalBrain>(Settings.World.numberOfWorldsToCreate, new ReverseDuplicateKeyComparer<float>());
            IAnimalBrain[] brainList = NeuralNetworkStorage.ReadFromFile(filePath);
            foreach (var brain in brainList)
            {
                sortedRabbitBrainList.Add(-1f, brain);
            }
            CreateAllWorlds();
        }

        public void SaveFoxBrainToFile(string filePath)
        {
            print("Save rabbit:  " + filePath);
            NeuralNetworkStorage.SaveToFile(filePath, sortedFoxesBrainList.Values.ToArray(), sortedFoxesBrainList.Keys.ToArray());
        }

        public void LoadFoxBrainFromFile(string filePath)
        {
            print("Load rabbit:  " + filePath);
            DestroyAllWorlds();
            sortedFoxesBrainList = new SortedList<float, IAnimalBrain>(Settings.World.numberOfWorldsToCreate, new ReverseDuplicateKeyComparer<float>());
            IAnimalBrain[] brainList = NeuralNetworkStorage.ReadFromFile(filePath);
            foreach (var brain in brainList)
            {
                sortedFoxesBrainList.Add(-1f, brain);
            }
            CreateAllWorlds();
        }

        private void FixedUpdate()
        {
            if (!Settings.Player.fastTrainingMode && RunSimulation)
            {
                UpdateAllWorlds();
                SetMoveableObjectIterators();
                foreach (Animal animal in animalIterator)
                {
                    animal.UpdatePosition();
                }
                animalIterator.Reset();
            }
        }

        private void Update()
        {
            if (Settings.Player.fastTrainingMode && RunSimulation)
            {
                UpdateAllWorlds();
                SetMoveableObjectIterators();
                foreach (Animal animal in animalIterator)
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
            sortedRabbitBrainList = new SortedList<float, IAnimalBrain>(Settings.World.numberOfWorldsToCreate, new ReverseDuplicateKeyComparer<float>());
            sortedFoxesBrainList = new SortedList<float, IAnimalBrain>(Settings.World.numberOfWorldsToCreate, new ReverseDuplicateKeyComparer<float>());
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
            sortedRabbitBrainList = new SortedList<float, IAnimalBrain>(Settings.World.numberOfWorldsToCreate, new ReverseDuplicateKeyComparer<float>());
            sortedFoxesBrainList = new SortedList<float, IAnimalBrain>(Settings.World.numberOfWorldsToCreate, new ReverseDuplicateKeyComparer<float>());
            CreateAllWorlds();
        }

        public void DestroyAllWorlds()
        {
            for (int i = worldGameObjects.Count - 1; i >= 0; i--)
            {
                Destroy(worldGameObjects[i].gameObject);
                worldGameObjects.RemoveAt(i);
            }

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

            for (int i = sortedRabbitBrainList.Count; i < Settings.World.numberOfWorldsToCreate; i++)
            {
                //sortedRabbitBrainList.Add(-1f, new NeuralNetwork(Settings.Rabbit.brainParams));
                sortedRabbitBrainList.Add(-1f, new DecisionTree(Settings.Rabbit.brainParams));
            }

            for (int i = sortedFoxesBrainList.Count; i < Settings.World.numberOfWorldsToCreate; i++)
            {
                //sortedFoxesBrainList.Add(-1f, new NeuralNetwork(Settings.Fox.brainParams));
                sortedFoxesBrainList.Add(-1f, new DecisionTree(Settings.Fox.brainParams));
            }

            WorldBuilder builder = worldOptions[UnityEngine.Random.Range(0, worldOptions.Count)];
            builder.StartNewBuild();
            for (int i = 0; i < Settings.World.numberOfWorldsToCreate; i++)
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
            foreach (World world in this.worlds)
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
            world.GetComponent<World>().RabbitBrain = sortedRabbitBrainList.Values[index];
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
            foreach (var line in Settings.World.worldDefinitionFile.text.Split('\n'))
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

            foreach (SimulationObjectPrefab prefabInfo in objectPrefabs)
            {
                prefabs.Add(prefabInfo.name, prefabInfo.prefab);
            }

            size = CalculateSize(Settings.World.numberOfWorldsToCreate);
        }
    }

    [System.Serializable]
    public struct SimulationObjectPrefab
    {
        public string name;
        public GameObject prefab;
    }
}