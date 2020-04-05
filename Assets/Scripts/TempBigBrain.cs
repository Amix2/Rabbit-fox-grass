using UnityEngine;
using UnityEditor;
using System.Threading;

public class TempBigBrain : IBigBrain
{
    public float Fitness { get; set; }

    public Vector3 GetDecision(float[] inputs)
    {
        Thread.Sleep(1);
        return new Vector3(0.5f, 0f, 0.5f);
    }
}