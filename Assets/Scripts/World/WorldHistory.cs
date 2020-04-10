using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class WorldHistory
    {
        private readonly Dictionary<Rabbit, RabbitHistory> aliveRabbits;
        public readonly List<Vector3> grassPositions;
        public readonly List<RabbitHistory> rabbits;
        public int lifeTime = 0;
        public Vector2Int worldSize;

        public WorldHistory()
        {
            grassPositions = new List<Vector3>();
            aliveRabbits = new Dictionary<Rabbit, RabbitHistory>();
            rabbits = new List<RabbitHistory>();
        }

        public void RabbitDeath(Rabbit rabbit, Vector3 pos)
        {
            if (!aliveRabbits.ContainsKey(rabbit))
            {
                throw new System.Exception("Rabbit has not yet been born");
            }
            RabbitHistory hist = aliveRabbits[rabbit];
            hist.deathPosition = pos;
            hist.lifeTime = lifeTime - hist.lifeTime;
            aliveRabbits.Remove(rabbit);
            rabbits.Add(hist);
        }

        public void RabbitBirth(Rabbit rabbit, Vector3 pos)
        {
            if(aliveRabbits.ContainsKey(rabbit))
            {
                throw new System.Exception("Rabbit is a Jezus, he was reborned");
            }
            aliveRabbits.Add(rabbit, new RabbitHistory { birthPosition = pos, deathPosition = Vector3.zero, foodEaten = 0f, lifeTime = lifeTime });
        }

        public void RabbitEat(Rabbit rabbit, float food)
        {
            if (!aliveRabbits.ContainsKey(rabbit))
            {
                throw new System.Exception("Rabbit has not yet been born");
            }
            var hist = aliveRabbits[rabbit];
            hist.foodEaten += food;
            aliveRabbits[rabbit] = hist;
        }

        public void Grass(Vector3 pos)
        {
            grassPositions.Add(pos);
        }

        public struct RabbitHistory
        {
            public Vector3 birthPosition, deathPosition;
            public int lifeTime;
            public float foodEaten;
        }
    }
}