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
        private List<Vector3> spawnPositionHistory;

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
            if (!Settings.World.allowedObjectNames.Contains(option[0].Trim()))
                throw new System.Exception(option[0] + " is not valid object name");

            objectsNames.Add(option[0].Trim());
            objectsCount.Add(int.Parse(option[1]));

            if (option[2].Trim() == "position")
                objectsPositions.Add(PositionSelector.FromPointList(option[3]));
            else if (option[2].Trim() == "circle")
                objectsPositions.Add(PositionSelector.FromCircleData(option[3]));
            else if (option[2].Trim() == "rectangle")
                objectsPositions.Add(PositionSelector.FromRectangleData(option[3]));
            else
                throw new System.Exception(option[2] + " is not valid input type");
        }

        public Vector2 CreateWorld(GameObject worldGO, Vector3 position, Transform parent, Dictionary<string, GameObject> prefabs)
        {
            World world = worldGO.GetComponent<World>();
            bool firstInGen = spawnPositionHistory.Count == 0;
            int spawnPosIndex = 0;
            world.Size = size;
            world.transform.Translate(position);
            for (int i = 0; i < objectsNames.Count; i++)
            {
                prefabs.TryGetValue(objectsNames[i], out GameObject prefab);
                for (int c = 0; c < objectsCount[i]; c++)   // spawn required amount of objects in random position from given list
                {
                    Vector3 pos;
                    if (firstInGen)
                    {
                        pos = objectsPositions[i].GetRandomPosition();
                        spawnPositionHistory.Add(pos);
                    }
                    else
                    {
                        pos = spawnPositionHistory[spawnPosIndex];
                        spawnPosIndex++;
                    }
                    if (objectsNames[i] == "rabbit")
                    {
                        world.AddRabbit(prefab, pos + new Vector3(0.5f, 0f, 0.5f));
                    }
                    else if (objectsNames[i] == "fox")
                    {
                        world.AddFox(prefab, pos + new Vector3(0.5f, 0f, 0.5f));
                    }
                    else if (objectsNames[i] == "grass")
                    {
                        world.AddGrass(prefab, pos + new Vector3(0.5f, 0f, 0.5f));
                    }
                }
            }

            world.Apply();
            return size;
        }

        public void StartNewBuild()
        {
            spawnPositionHistory = new List<Vector3>();
        }

        public Vector2 ResetWorld(GameObject world, Vector3 position, Transform parent, SimulationObjectPrefab[] prefabs)
        {
            return position;
        }
    }
}

internal class PositionSelector
{
    private readonly List<Vector2> positions;

    public static PositionSelector FromPointList(string list)
    {
        PositionSelector selector = new PositionSelector(list.Split('-').Length);
        foreach (string values in list.Split('-'))
        {
            var valuesArr = values.Split(',');
            selector.AddPosition(new Vector2(float.Parse(valuesArr[0]), float.Parse(valuesArr[1])));
        }
        return selector;
    }

    // centerX,centerY - radius
    public static PositionSelector FromCircleData(string circleData)
    {
        PositionSelector selector = new PositionSelector();
        string[] values = circleData.Split('-');
        string[] centerValues = values[0].Split(',');
        Vector2 center = new Vector2(float.Parse(centerValues[0]), float.Parse(centerValues[1]));
        float radius = float.Parse(values[1]);
        for (float angle = 0; angle < 360; angle += 1)
        {
            Vector2 newPoint = new Vector2(Mathf.Round(center.x + Mathf.Sin(angle) * radius), Mathf.Round(center.y + Mathf.Cos(angle) * radius));
            if (!selector.positions.Contains(newPoint))
            {
                selector.positions.Add(newPoint);
            }
        }
        return selector;
    }

    // minX,minY - maxX,maxY
    public static PositionSelector FromRectangleData(string list)
    {
        PositionSelector selector = new PositionSelector();
        string[] twoPoints = list.Split('-');
        int minX = int.Parse(twoPoints[0].Split(',')[0]);
        int minY = int.Parse(twoPoints[0].Split(',')[1]);
        int maxX = int.Parse(twoPoints[1].Split(',')[0]);
        int maxY = int.Parse(twoPoints[1].Split(',')[1]);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                selector.AddPosition(new Vector2(x, y));
            }
        }

        return selector;
    }

    private PositionSelector(int size)
    {
        positions = new List<Vector2>(size);
    }

    private PositionSelector()
    {
        positions = new List<Vector2>();
    }

    public Vector3 GetRandomPosition()
    {
        var retPos = positions[Random.Range(0, positions.Count)];
        return new Vector3(retPos.x, 0f, retPos.y);
    }

    private void AddPosition(Vector2 pos)
    {
        positions.Add(pos);
    }
}