using UnityEngine;
using UnityEditor;

public class TempBigBrain : IBigBrain
{
    public Vector3 GetDecision(float[] inputs)
    {
        return new Vector3(0.5f, 0f, 0.5f);
    }
}