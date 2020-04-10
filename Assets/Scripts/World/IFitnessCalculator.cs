using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public abstract class IFitnessCalculator
    {
        abstract public float CalculateFitness(WorldHistory worldHistory);

        protected static float SqrDistaceToClosestGrass(List<Vector3> grassPositions, Vector3 rabbitPos)
        {
            float sqrDistanceToClosest = float.MaxValue;
            foreach (Vector3 grassPos in grassPositions)
            {
                float sqrMagnitude = (grassPos - rabbitPos).sqrMagnitude;
                if (sqrDistanceToClosest > sqrMagnitude)
                {
                    sqrDistanceToClosest = sqrMagnitude;
                }
            }

            return sqrDistanceToClosest;
        }
    }
}
