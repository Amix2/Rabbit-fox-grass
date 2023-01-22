using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public enum RabbitFitnessCalculatorOptions
    {
        FoodAndDistanceToClosestGrass,
        FoodAndAvgDistanceToClosestGrass,
    }

    public enum FoxFitnessCalculatorOptions
    {
        FoodAndAvgDistanceToClosestRabbitInFrame
    }

    internal static class FitnessCalculatorOptionsExtension
    {
        public static IFitnessCalculator GetCalculator(this RabbitFitnessCalculatorOptions option)
        {
            switch (option)
            {
                case RabbitFitnessCalculatorOptions.FoodAndDistanceToClosestGrass: return new Rabit_FoodAndDistanceToClosestGrass();
                case RabbitFitnessCalculatorOptions.FoodAndAvgDistanceToClosestGrass: return new Rabit_FoodAndAvgDistanceToClosestGrass();
            }
            return new DefaultCalculator();
        }

        public static IFitnessCalculator GetCalculator(this FoxFitnessCalculatorOptions option)
        {
            switch (option)
            {
                case FoxFitnessCalculatorOptions.FoodAndAvgDistanceToClosestRabbitInFrame: return new Fox_FoodAndAvgDistanceToClosestRabbitInFrame();
            }
            return new DefaultCalculator();
        }
    }

    public class Rabit_FoodAndDistanceToClosestGrass : IFitnessCalculator
    {
        // AVG ( time * (food+1) + k / distToClosestGrass )

        public override float CalculateFitness(WorldHistory worldHistory)
        {
            if (!Settings.World.collectHistory || worldHistory.rabbits.Count == 0) return 0f;
            float scoreSum = 0f;
            foreach (AnimalHistory rabbitHist in worldHistory.rabbits)
            {
                float sqrDistanceToClosest = SqrDistaceToClosestGrass(worldHistory.grassPositions, rabbitHist.Positions);
                scoreSum += (rabbitHist.FoodEaten + 1f) * (worldHistory.worldSize.sqrMagnitude - sqrDistanceToClosest) / worldHistory.worldSize.sqrMagnitude;
            }
            float scoreAvg = scoreSum / worldHistory.rabbits.Count;

            return scoreAvg;
        }
    }

    public class Rabit_FoodAndAvgDistanceToClosestGrass : IFitnessCalculator
    {
        // AVG ( time * (food+1) + k / distToClosestGrass )

        public override float CalculateFitness(WorldHistory worldHistory)
        {
            if (!Settings.World.collectHistory || worldHistory.rabbits.Count == 0) return 0f;
            float scoreSum = 0f;
            foreach (AnimalHistory rabbitHist in worldHistory.rabbits)
            {
                float sqrAvgDistanceToClosest = AvgSqrDistaceToClosestTarget(worldHistory.grassPositions, rabbitHist.Positions);
                scoreSum += (rabbitHist.LifeTime + rabbitHist.FoodEaten) * (worldHistory.worldSize.sqrMagnitude - sqrAvgDistanceToClosest) / worldHistory.worldSize.sqrMagnitude;
            }
            float scoreAvg = scoreSum / worldHistory.rabbits.Count;

            return scoreAvg;
        }
    }

    public class Fox_FoodAndAvgDistanceToClosestRabbitInFrame : IFitnessCalculator
    {
        public override float CalculateFitness(WorldHistory worldHistory)
        {
            if (!Settings.World.collectHistory || worldHistory.foxes.Count == 0) return 0f;

            // value[i] => rabbit positions at time 'i'
            List<List<Vector3>> rabbitsPositionsInTime = new List<List<Vector3>>();

            foreach (AnimalHistory rabbitHisory in worldHistory.rabbits)
            {
                // if positionsInTime list is too small
                if (rabbitHisory.DeathTime >= rabbitsPositionsInTime.Count)
                {
                    // calculate how many emelents needs to be added
                    int addMore = rabbitHisory.DeathTime - rabbitsPositionsInTime.Count + 1;

                    // help list, prevents excessive copy on positionInTime list
                    List<List<Vector3>> addList = new List<List<Vector3>>(addMore);
                    for (int i = 0; i < addMore; i++)
                    {
                        addList.Add(new List<Vector3>());
                    }
                    // add range to make sure only 1 copy
                    rabbitsPositionsInTime.AddRange(addList);
                }

                // now rabbitsPositionsInTime has enough space for positions of given rabbit

                // add rabbit positions to the storage
                for (int time = rabbitHisory.BirthTime; time <= rabbitHisory.DeathTime; time++)
                {
                    // add every position in time in correct place in rabbitsPositionsInTime
                    if (rabbitHisory.PositionInTime(time, out Vector3 pos))
                        rabbitsPositionsInTime[time].Add(pos);
                    else
                        throw new System.Exception("FIX ME!!!");
                }
            }

            // now rabbitsPositionsInTime[t] has list of rabbit positions in time 't'

            float scoreSum = 0f;
            float maxDistance = worldHistory.worldSize.sqrMagnitude;
            // for each fox
            foreach (AnimalHistory foxHistory in worldHistory.foxes)
            {
                float sumSqrDistanceInTime = 0;
                int sqrDistCount = 0;
                // for every time
                for (int t = foxHistory.BirthTime; t <= foxHistory.DeathTime; t++)
                {
                    // if no rabbits were alive at that point in time
                    if (t >= rabbitsPositionsInTime.Count)
                    {
                        continue;
                    }

                    sqrDistCount++;

                    // prepare foxPosition and rabbit positions
                    if (!foxHistory.PositionInTime(t, out Vector3 foxPos))
                        throw new System.Exception("FIX ME!!!");
                    List<Vector3> rabbitPositions = rabbitsPositionsInTime[t];

                    // find closest distance fox - rabbit in given time
                    float minSqrDistToRabbit = maxDistance;
                    foreach (Vector3 rabbitPos in rabbitPositions)
                    {
                        float currSqrDist = (rabbitPos - foxPos).sqrMagnitude;
                        if (minSqrDistToRabbit > currSqrDist) minSqrDistToRabbit = currSqrDist;
                    }

                    // add to the sum over every time
                    sumSqrDistanceInTime += minSqrDistToRabbit;
                }

                float avgSqrDistanceInTime = sumSqrDistanceInTime / sqrDistCount;

                // add to score sum over every fox
                scoreSum += (foxHistory.FoodEaten + 1f) * (worldHistory.worldSize.sqrMagnitude - avgSqrDistanceInTime) / worldHistory.worldSize.sqrMagnitude;
            }

            float scoreAvg = scoreSum / worldHistory.foxes.Count;
            return scoreAvg;
        }

        private float AvgSqrDistaceToClosestTarget(List<List<Vector3>> targetPositionsInTime, List<Vector3> sourcePositions)
        {
            if (sourcePositions.Count == 0) return 0;
            float distSqrSum = 0;
            for (int i = 0; i < Mathf.Min(targetPositionsInTime.Count, sourcePositions.Count); i++)
            {
                if (targetPositionsInTime[i].Count == 0) continue;
                float curSqrDist = float.MaxValue;
                foreach (Vector3 pos in targetPositionsInTime[i])
                {
                    if (curSqrDist > (pos - sourcePositions[i]).sqrMagnitude) curSqrDist = (pos - sourcePositions[i]).sqrMagnitude;
                }
                distSqrSum += curSqrDist;
            }
            return distSqrSum / sourcePositions.Count;
        }
    }

    public class DefaultCalculator : IFitnessCalculator
    {
        public override float CalculateFitness(WorldHistory worldHistory)
        {
            Debug.LogError("Fitness Calculator not set");
            return -1f;
        }
    }
}