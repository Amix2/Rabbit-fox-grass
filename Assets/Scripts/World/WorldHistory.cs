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

        public WorldHistory(MultiTypeEventHandler<float, int, Vector3> worldEvents)
        {
            grassPositions = new List<Vector3>();
            aliveRabbits = new Dictionary<Rabbit, RabbitHistory>();
            rabbits = new List<RabbitHistory>();
            worldEvents.Subscribe(HistoryEventType.TURN_UPDATE, (object sender, int amount) => UpdateTurnEvent(amount));
            worldEvents.Subscribe(HistoryEventType.DEATH, (object sender, Vector3 position) => HandleDeathEvent(sender, position));
            worldEvents.Subscribe(HistoryEventType.BIRTH, (object sender, Vector3 position) => HandleBirthEvent(sender, position));
            worldEvents.Subscribe(HistoryEventType.EAT, (object sender, float amount) => HandleEatEvent(sender, amount));
        }

        private void UpdateTurnEvent(int amount)
        {
            lifeTime += amount;
        }

        private void HandleDeathEvent(object obj, Vector3 position)
        {
            if (typeof(Grass).IsInstanceOfType(obj))
            {
                AddGrass(position);
            }
            else if (typeof(Rabbit).IsInstanceOfType(obj))
            {
                RabbitDeath(obj as Rabbit, position);
            }
        }

        private void HandleBirthEvent(object obj, Vector3 position)
        {
            if (typeof(Grass).IsInstanceOfType(obj))
            {
                AddGrass(position);
            }
            else if (typeof(Rabbit).IsInstanceOfType(obj))
            {
                RabbitBirth(obj as Rabbit, position);
            }
        }

        private void HandleEatEvent(object obj, float amount)
        {
            if (typeof(Rabbit).IsInstanceOfType(obj))
            {
                RabbitEat(obj as Rabbit, amount);
            }
        }

        private void RabbitDeath(Rabbit rabbit, Vector3 pos)
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

        private void RabbitBirth(Rabbit rabbit, Vector3 pos)
        {
            if (aliveRabbits.ContainsKey(rabbit))
            {
                throw new System.Exception("Rabbit is a Jezus, he was reborned");
            }
            aliveRabbits.Add(rabbit, new RabbitHistory { birthPosition = pos, deathPosition = Vector3.zero, foodEaten = 0f, lifeTime = lifeTime });
        }

        private void RabbitEat(Rabbit rabbit, float food)
        {
            if (!aliveRabbits.ContainsKey(rabbit))
            {
                throw new System.Exception("Rabbit has not yet been born");
            }
            var hist = aliveRabbits[rabbit];
            hist.foodEaten += food;
            aliveRabbits[rabbit] = hist;
        }

        private void AddGrass(Vector3 pos)
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