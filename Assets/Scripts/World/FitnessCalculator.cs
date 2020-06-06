using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
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
            foreach (WorldHistory.AnimalHistory rabbitHist in worldHistory.rabbits)
            {
                float sqrDistanceToClosest = SqrDistaceToClosestGrass(worldHistory.grassPositions, rabbitHist.positions);
                scoreSum += (rabbitHist.foodEaten + 1f) * (worldHistory.worldSize.sqrMagnitude - sqrDistanceToClosest) / worldHistory.worldSize.sqrMagnitude;
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
            foreach (WorldHistory.AnimalHistory rabbitHist in worldHistory.rabbits)
            {
                float sqrAvgDistanceToClosest = AvgSqrDistaceToClosestTarget(worldHistory.grassPositions, rabbitHist.positions);
                scoreSum += (rabbitHist.foodEaten + 1f) * (worldHistory.worldSize.sqrMagnitude - sqrAvgDistanceToClosest) / worldHistory.worldSize.sqrMagnitude;
            }
            float scoreAvg = scoreSum / worldHistory.rabbits.Count;

            return scoreAvg;
        }
    }

    [Description("Assumes all animals are born in the same moment")]
    public class Fox_FoodAndAvgDistanceToClosestRabbitInFrame : IFitnessCalculator
    {

        public override float CalculateFitness(WorldHistory worldHistory)
        {
            if (!Settings.World.collectHistory || worldHistory.foxes.Count == 0) return 0f;
            List<List<Vector3>> rabbitsPositionsInTime = new List<List<Vector3>>();
            List<WorldHistory.AnimalHistory> rabbits = worldHistory.rabbits.ToList();
            int it = 0;
            while(rabbits.Count > 0)
            {
                rabbitsPositionsInTime.Add(new List<Vector3>());
                for (int i=rabbits.Count-1; i>=0; i--)
                {
                    if(rabbits[i].positions.Count > it)
                    {
                        rabbitsPositionsInTime[rabbitsPositionsInTime.Count - 1].Add(rabbits[i].positions[it]);
                    } else
                    {
                        rabbits.RemoveAt(i);
                    }
                }
                it++;
            }
            float scoreSum = 0f;
            foreach (WorldHistory.AnimalHistory foxHistory in worldHistory.foxes)
            {
                float sqrAvgDistanceToClosest = AvgSqrDistaceToClosestTarget(rabbitsPositionsInTime, foxHistory.positions);
                scoreSum += (foxHistory.foodEaten + 1f) * (worldHistory.worldSize.sqrMagnitude - sqrAvgDistanceToClosest) / worldHistory.worldSize.sqrMagnitude;
            }
            float scoreAvg = scoreSum / worldHistory.foxes.Count;

            return scoreAvg;
        }

        private float AvgSqrDistaceToClosestTarget(List<List<Vector3>> targetPositionsInTime, List<Vector3> sourcePositions)
        {
            if (sourcePositions.Count == 0) return 0;
            float distSqrSum = 0;
            for(int i=0; i<Mathf.Min(targetPositionsInTime.Count, sourcePositions.Count); i++)
            {
                if (targetPositionsInTime[i].Count == 0) continue;
                float curSqrDist = float.MaxValue;
                foreach(Vector3 pos in targetPositionsInTime[i])
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