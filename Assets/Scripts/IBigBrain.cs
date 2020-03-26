using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBigBrain
{
    Vector3 GetDecision(float[] inputs);
}
