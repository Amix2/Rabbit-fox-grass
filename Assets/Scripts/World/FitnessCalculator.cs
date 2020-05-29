using UnityEngine;

namespace World
{
    public enum FitnessCalculatorOptions
    {
        RabitAvg_FoodAndDistanceToClosestGrass,
        RabitAvg_FoodAndAvgDistanceToClosestGrass
    }

    internal static class FitnessCalculatorOptionsExtension
    {
        public static IFitnessCalculator GetCalculator(this FitnessCalculatorOptions option)
        {
            switch (option)
            {
                case FitnessCalculatorOptions.RabitAvg_FoodAndDistanceToClosestGrass: return new RabitAvg_FoodAndDistanceToClosestGrass();
                case FitnessCalculatorOptions.RabitAvg_FoodAndAvgDistanceToClosestGrass: return new RabitAvg_FoodAndAvgDistanceToClosestGrass();
            }
            return new DefaultCalculator();
        }
    }

    public class RabitAvg_FoodAndDistanceToClosestGrass : IFitnessCalculator
    {
        // AVG ( time * (food+1) + k / distToClosestGrass )

        public override float CalculateFitness(WorldHistory worldHistory)
        {
            if (!Settings.World.collectHistory || worldHistory.rabbits.Count == 0) return 0f;
            float scoreSum = 0f;
            foreach (WorldHistory.RabbitHistory rabbitHist in worldHistory.rabbits)
            {
                float sqrDistanceToClosest = SqrDistaceToClosestGrass(worldHistory.grassPositions, rabbitHist.positions);
                scoreSum += (rabbitHist.foodEaten + 1f) * (worldHistory.worldSize.sqrMagnitude - sqrDistanceToClosest) / worldHistory.worldSize.sqrMagnitude;
            }
            float scoreAvg = scoreSum / worldHistory.rabbits.Count;

            return scoreAvg;
        }
    }

    public class RabitAvg_FoodAndAvgDistanceToClosestGrass : IFitnessCalculator
    {
        // AVG ( time * (food+1) + k / distToClosestGrass )

        public override float CalculateFitness(WorldHistory worldHistory)
        {
            if (!Settings.World.collectHistory || worldHistory.rabbits.Count == 0) return 0f;
            float scoreSum = 0f;
            foreach (WorldHistory.RabbitHistory rabbitHist in worldHistory.rabbits)
            {
                float sqrAvgDistanceToClosest = AvgSqrDistaceToClosestGrass(worldHistory.grassPositions, rabbitHist.positions);
                scoreSum += (rabbitHist.foodEaten + 1f) * (worldHistory.worldSize.sqrMagnitude - sqrAvgDistanceToClosest) / worldHistory.worldSize.sqrMagnitude;
            }
            float scoreAvg = scoreSum / worldHistory.rabbits.Count;

            return scoreAvg;
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