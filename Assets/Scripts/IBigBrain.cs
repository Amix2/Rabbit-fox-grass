using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

public interface IBigBrain
{
    Vector3 GetDecision(float[] inputs);
}
