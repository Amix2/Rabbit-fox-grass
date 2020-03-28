using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class WorldCreator : MonoBehaviour
    {
        public GameObject worldPrefab;
        public SimulationObjectPrefab[] objectPrefabs;
        public TextAsset worldDefinitionFile;
        public int numberOfWorldsToCreate;

        private IBigBrain bigBrain;
        private List<WorldBuilder> worldOptions;
        private List<GameObject> worldGameObjects;
        private List<World> worlds;
        private Dictionary<string, GameObject> prefabs;
        private Vector2Int size;
        private float gapBetweenWorlds;

        private void Start()
        {
            gapBetweenWorlds = 1;
            // Create field objects
            InitResources();
            // Fill List<WorldBuilder> worldOptions from input file (TextAsset worldDefinitionFile)
            ParseInputFile();
            // Create worlds based on int numberOfWorldsToCreate
            CreateAllWorlds();
        }

        private void CreateAllWorlds()
        {
            Vector3 offset = Vector3.zero;  // offset to bottom-left corner
            int worldsOnX = 0;
            for (int i = 0; i < numberOfWorldsToCreate; i++)
            {
                GameObject world = CreateWorld(ref offset, ref worldsOnX, i);
                worldGameObjects.Add(world);
                world.GetComponent<World>().bigBrain = bigBrain;
                worlds.Add(world.GetComponent<World>());
            }
        }

        private GameObject CreateWorld(ref Vector3 offset, ref int worldsOnX, int i)
        {
            int builderIndex = Random.Range(0, worldOptions.Count);
            WorldBuilder builder = worldOptions[builderIndex];
            GameObject world = Instantiate(worldPrefab);
            world.name = "World_" + i;
            var lastSize = builder.CreateWorld(world, offset, null, prefabs);   // Apply setup from file to giver world
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
                //try
                //{
                    worldOptions.Add(new WorldBuilder(line));
                //}
                //catch (System.Exception e)
                //{
                //    Debug.LogError(e);
                //}
            }
        }

        private void InitResources()
        {
            bigBrain = new TempBigBrain();
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

        private void UpdateBehaviourAllWorlds()
        {
            foreach (World world in worlds)
            {
                world.UpdateBehaviour();
            }
        }

        private void FixedUpdate()
        {
            if (!Settings.Player.fastTrainingMode)
            {
                UpdateBehaviourAllWorlds();
            }
        }

        private void Update()
        {
            if (Settings.Player.fastTrainingMode)
            {
                UpdateBehaviourAllWorlds();
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
    }

    [System.Serializable]
    public struct SimulationObjectPrefab
    {
        public string name;
        public GameObject prefab;
    }
}