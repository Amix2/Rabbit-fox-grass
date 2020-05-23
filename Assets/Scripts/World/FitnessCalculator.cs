using UnityEngine;

namespace World
{
    public enum FitnessCalculatorOptions
    {
        RabitAvg_TimeFoodAndDistanceToClosestGrass,
    }

    internal static class FitnessCalculatorOptionsExtension
    {
        public static IFitnessCalculator GetCalculator(this FitnessCalculatorOptions option)
        {
            switch (option)
            {
                case FitnessCalculatorOptions.RabitAvg_TimeFoodAndDistanceToClosestGrass: return new RabitAvg_TimeFoodAndDistanceToClosestGrass();
            }
            return new DefaultCalculator();
        }
    }

    public class RabitAvg_TimeFoodAndDistanceToClosestGrass : IFitnessCalculator
    {
        // AVG ( time * (food+1) + k / distToClosestGrass )

        public override float CalculateFitness(WorldHistory worldHistory)
        {
            if (worldHistory.rabbits.Count == 0) return 0f;
            float scoreSum = 0f;
            foreach (var rabbitHist in worldHistory.rabbits)
            {
                Vector3 rabbitPos = rabbitHist.deathPosition;
                float sqrDistanceToClosest = SqrDistaceToClosestGrass(worldHistory.grassPositions, rabbitPos);
                scoreSum += (rabbitHist.foodEaten + 1f) * worldHistory.worldSize.sqrMagnitude / (worldHistory.worldSize.sqrMagnitude - sqrDistanceToClosest);
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