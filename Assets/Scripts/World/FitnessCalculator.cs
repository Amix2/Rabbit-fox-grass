using UnityEngine;
using System.Collections;
namespace World
{

    public enum FitnessCalculatorOptions
    {
        Time_RabbitToClosestGrass,
        Time_RabbitToAverageGrass
    }

    static class FitnessCalculatorOptionsExtension
    {
        public static IFitnessCalculator GetCalculator(this FitnessCalculatorOptions option)
        {
            switch (option)
            {
                case FitnessCalculatorOptions.Time_RabbitToClosestGrass: return new Time_RabbitToClosestGrass();
                case FitnessCalculatorOptions.Time_RabbitToAverageGrass: return new Time_RabbitToAverageGrass();
            }
            return new DefaultCalculator();
        }
    }

    public class Time_RabbitToClosestGrass : IFitnessCalculator
    {
        // lifeTime +  sum( distance( rabbit, closest grass )^2 ) 
        public float CalculateFitness(WorldHistory worldHistory)
        {
            float sqrDistanceSum = 0f;
            foreach(Vector3 rabbitPos in worldHistory.rabbitsDeath)
            {
                float sqrDistanceToClosest = float.MaxValue;
                foreach(Vector3 grassPos in worldHistory.grassPositions)
                {
                    float sqrMagnitude = (grassPos - rabbitPos).sqrMagnitude;
                    if(sqrDistanceToClosest > sqrMagnitude)
                    {
                        sqrDistanceToClosest = sqrMagnitude;
                    }
                }
                sqrDistanceSum += sqrDistanceToClosest;
            }
            return worldHistory.lifeTime + sqrDistanceSum;
        }
    }

    public class Time_RabbitToAverageGrass : IFitnessCalculator
    {
        // lifeTime +  sum( distance( rabbit, closest grass )^2 ) 
        public float CalculateFitness(WorldHistory worldHistory)
        {
            float sqrDistanceSum = 0f;
            foreach (Vector3 rabbitPos in worldHistory.rabbitsDeath)
            {
                float sqrDistance = 0;
                foreach (Vector3 grassPos in worldHistory.grassPositions)
                {
                    sqrDistance += (grassPos - rabbitPos).sqrMagnitude;
                }
                sqrDistanceSum += sqrDistance / worldHistory.grassPositions.Count;
            }
            return worldHistory.lifeTime + sqrDistanceSum;
        }
    }

    public class DefaultCalculator : IFitnessCalculator
    {
        public float CalculateFitness(WorldHistory worldHistory)
        {
            throw new System.Exception("Fitness Calculator not set");
        }
    }
}


