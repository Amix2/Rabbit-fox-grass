﻿namespace World
{
    public interface IEdible
    {
        float FoodAmount { get; }

        float Consumed(float amount = 1f);
    }
}