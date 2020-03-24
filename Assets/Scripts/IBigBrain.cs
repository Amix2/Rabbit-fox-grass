using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBigBrain
{
    Vector2 GetDecision(float[] inputs);
}
