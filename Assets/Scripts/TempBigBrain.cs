using UnityEngine;
using UnityEditor;

public class TempBigBrain : IBigBrain
{
    public Vector2 GetDecision(float[] inputs)
    {
        return new Vector2(0.5f, 0.5f);
    }
}