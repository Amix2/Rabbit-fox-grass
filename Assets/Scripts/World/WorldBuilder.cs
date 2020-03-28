using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace World
{
    internal class WorldBuilder
    {
        private Vector2Int size;
        private readonly List<string> objectsNames;
        private readonly List<int> objectsCount;
        private readonly List<PositionSelector> objectsPositions;

        public WorldBuilder(string options) // example options = " 7,7 | rabbit : 1 : position : 3,3 | grass : 1 : position : 2,2 - 3,2 - 4,2 - 4,3 - 4,4 - 3,4 - 2,4  - 2,3 "
        {
            try
            {
                objectsNames = new List<string>();
                objectsPositions = new List<PositionSelector>();
                objectsCount = new List<int>();

                string[] optionsArr = options.Split('|');

                // parse size, first parameter
                string[] sizeStr = optionsArr[0].Split(',');
                size = new Vector2Int(int.Parse(sizeStr[0]), int.Parse(sizeStr[1]));

                // parse other, one by one
                for (int i = 1; i < optionsArr.Length; i++)
                {
                    ParseOneOption(optionsArr, i);
                }
            }
            catch (System.Exception e)
            {
                throw new System.Exception(options + " is not valid world definition, error: " + e.Message);
            }
        }

        private void ParseOneOption(string[] optionsArr, int i)
        {
            string[] option = optionsArr[i].Split(':');

            // check if object name is allowed (set allowed in Settings)
            if (!Settings.Player.allowedObjectNames.Contains(option[0].Trim()))
            {
                throw new System.Exception(option[0] + " is not valid object name");
            }

            objectsNames.Add(option[0].Trim());
            objectsCount.Add(int.Parse(option[1]));
            if (option[2].Trim() == "position")
            {
                objectsPositions.Add(PositionSelector.FromList(option[3]));
            }
            else
            {
                throw new System.Exception(option[2] + " is not valid input type");
            }
        }

        public Vector2 CreateWorld(GameObject worldGO, Vector3 position, Transform parent, Dictionary<string, GameObject> prefabs)
        {
            World world = worldGO.GetComponent<World>();

            world.size = size;
            world.transform.Translate(position);
            for (int i = 0; i < objectsNames.Count; i++)
            {
                GameObject prefab;
                prefabs.TryGetValue(objectsNames[i], out prefab);
                for (int c = 0; c < objectsCount[i]; c++)
                {
                    if (objectsNames[i] == "rabbit")
                    {
                        world.AddRabbit(prefab, objectsPositions[i].GetRandomPosition() + new Vector3(0.5f, 0f, 0.5f));
                    }
                    else if (objectsNames[i] == "grass")
                    {
                        world.AddGrass(prefab, objectsPositions[i].GetRandomPosition() + new Vector3(0.5f, 0f, 0.5f));
                    }
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

internal class PositionSelector
{
    private Vector2[] positions;
    private int ind = 0;

    public static PositionSelector FromList(string list)
    {
        PositionSelector selector = new PositionSelector(list.Split('-').Length);
        foreach (string values in list.Split('-'))
        {
            var valuesArr = values.Split(',');
            selector.AddPosition(new Vector2(float.Parse(valuesArr[0]), float.Parse(valuesArr[1])));
        }
        return selector;
    }

    private PositionSelector(int size)
    {
        positions = new Vector2[size];
    }

    public Vector3 GetRandomPosition()
    {
        var retPos = positions[Random.Range(0, positions.Length)];
        return new Vector3(retPos.x, 0f, retPos.y);
    }

    private void AddPosition(Vector2 pos)
    {
        positions[ind] = pos;
        ind++;
    }
}