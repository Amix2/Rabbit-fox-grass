
using System;
using System.Text;
using UnityEngine;

public class IAnimalBrain
{
    public virtual Vector3 GetDecision(float[] floats)
    {
        throw new NotImplementedException();
    }

    public virtual int GetSegmentCount()
    {
        throw new NotImplementedException();
    }

    public virtual void AddToFile(float fitness, StringBuilder stringBuilder)
    {
        throw new NotImplementedException();
    }

    public virtual void AsBestToFile(StringBuilder stringBuilder)
    {
        throw new NotImplementedException();
    }

}
