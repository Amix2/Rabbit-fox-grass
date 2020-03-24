using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace World
{

    public class WorldCreator : MonoBehaviour
    {
        public GameObject worldPrefab;
        public SimulationObjectPrefab[] objectPrefabs;
        public TextAsset worldDefinitionFile;
        public int numberOfWorldsToCreate;

        IBigBrain bigBrain;
        List<WorldBuilder> worldOptions;
        List<GameObject> worldGameObjects;
        List<World> worlds;
        public Dictionary<string, GameObject> prefabs;

        void Start()
        {
            bigBrain = new TempBigBrain();
            worldGameObjects = new List<GameObject>();
            worlds = new List<World>();
            prefabs = new Dictionary<string, GameObject>();

            foreach(var prefabInfo in objectPrefabs)
            {
                prefabs.Add(prefabInfo.name, prefabInfo.prefab);
            }

            worldOptions = new List<WorldBuilder>();
            foreach (var line in worldDefinitionFile.text.Split('\n'))
            {
                try
                {
                    worldOptions.Add(new WorldBuilder(line));
                } catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
            }

            for(int i=0; i<numberOfWorldsToCreate; i++)
            {
                int builderIndex = Random.Range(0, worldOptions.Count - 1);
                WorldBuilder builder = worldOptions[builderIndex];
                GameObject world = Instantiate(worldPrefab);
                world.name = "World_" + i;
                builder.CreateWorld(world, Vector3.zero, null, prefabs);

                worldGameObjects.Add(world);
                worlds.Add(world.GetComponent<World>());
            }
        }

        void UpdateAllWorlds()
        {
            foreach(World world in worlds)
            {
                world.UpdateBehaviour();
            }
        }
    }

   






    [System.Serializable]
    public struct SimulationObjectPrefab
    {
        public string name;
        public GameObject prefab;
    }

}