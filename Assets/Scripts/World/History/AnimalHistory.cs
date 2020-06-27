using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class AnimalHistory
    {
        private int birthTime = -1;
        private int deathTime = -1;
        private float foodEaten = 0;
        private readonly List<Vector3> positions = new List<Vector3>();

        public Vector3 BirthPosition { get => Positions[0]; }
        public Vector3 DeathPosition { get => Positions[Positions.Count - 1]; }
        public int LifeTime { get => deathTime - birthTime + 1; }
        public float FoodEaten { get => foodEaten; set => foodEaten = value; }
        public List<Vector3> Positions { get => positions; }
        public int BirthTime { get => birthTime; set => birthTime = value; }
        public int DeathTime { get => deathTime; set => deathTime = value; }

        public bool PositionInTime(int time, out Vector3 position)
        {
            if (time < birthTime || time > deathTime)
            {
                position = default;
                return false;
            }
            else
            {
                int ind = time - birthTime;
                position = Positions[ind];
                return true;
            }
        }
    }
}