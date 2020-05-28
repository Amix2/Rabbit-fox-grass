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

        protected static float SqrDistaceToClosestGrass(List<Vector3> grassPositions, List<Vector3> rabbitPositions)
        {
            if (rabbitPositions.Count == 0) return 0;

            float sqrDistanceToClosest = float.MaxValue;
            foreach (Vector3 grassPos in grassPositions)
            {
                foreach (Vector3 rabbitPos in rabbitPositions)
                {
                    float sqrMagnitude = (grassPos - rabbitPos).sqrMagnitude;
                    if (sqrDistanceToClosest > sqrMagnitude)
                    {
                        sqrDistanceToClosest = sqrMagnitude;
                    }

                }
            }
            return sqrDistanceToClosest;
        }

        protected static float AvgSqrDistaceToClosestGrass(List<Vector3> grassPositions, List<Vector3> rabbitPositions)
        {
            if (rabbitPositions.Count == 0) return 0;

            float sqrSumPos = 0;
            foreach (Vector3 rabbitPos in rabbitPositions)
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
                sqrSumPos += sqrDistanceToClosest;
            }
            return sqrSumPos / rabbitPositions.Count;
        }
    }
}
