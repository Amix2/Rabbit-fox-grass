using MathNet.Numerics.Random;
using System;

public static class Utils
{
    private static readonly MersenneTwister random = new MersenneTwister();

    // Returns float in range (min, max)
    public static float FloatInRange(float min, float max)
    {
        //return min + 0.5f * (max - min);
        return min + RandomFloat() * (max - min);
    }

    // Returns float in range (0.0, 1.0)
    public static float RandomFloat()
    {
        //return 0.5f;
        MersenneTwister random = new MersenneTwister();
        return Convert.ToSingle(random.NextDouble());
    }
}