using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace World
{

    internal class WorldBuilder
    {
        Vector2Int size;
        List<string> objectsNames;
        List<Vector3> objectsPositions;

        public WorldBuilder(string options)
        {
            try
            {
                objectsNames = new List<string>();
                objectsPositions = new List<Vector3>();

                string[] optionsArr = options.Split(',');

                string[] sizeStr = optionsArr[0].Trim().Split(':');
                size = new Vector2Int(int.Parse(sizeStr[0]), int.Parse(sizeStr[1]));
                for (int i = 1; i < optionsArr.Length; i++)
                {
                    string[] opt = optionsArr[i].Trim().Split(':');
                    objectsNames.Add(opt[0].Trim());
                    objectsPositions.Add(new Vector3(int.Parse(opt[1]), 0f, int.Parse(opt[2])));
                }

            } catch (System.Exception e)
            {
                throw new System.Exception(options + " is not valid world definition, error: " + e.Message);
            }

        }

        public Vector2 CreateWorld(GameObject worldGO, Vector3 position, Transform parent, Dictionary<string, GameObject> prefabs)
        {
            World world = worldGO.GetComponent<World>();

            world.size = size;
            world.transform.Translate(position);

            for (int i=0; i<objectsNames.Count; i++)
            {
                GameObject prefab;
                prefabs.TryGetValue(objectsNames[i], out prefab);
                if(objectsNames[i] == "rabbit")
                {
                    world.AddRabbit(prefab, objectsPositions[i]);
                } else if (objectsNames[i] == "grass")
                {
                    world.AddGrass(prefab, objectsPositions[i]);
                }
            }

            world.Apply();
            return size;
        }

        public Vector2 ResetWorld(GameObject world, Vector3 position, Transform parent, SimulationObjectPrefab[] prefabs)
        {
            return position;
        }
    }
}
