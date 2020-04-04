using NumSharp;
using UnityEngine;

public interface IBigBrain
{
    Vector3 GetDecision(NDArray inputs);
}