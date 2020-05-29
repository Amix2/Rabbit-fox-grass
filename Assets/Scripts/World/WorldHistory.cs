using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class WorldHistory
    {
        private readonly ConcurrentDictionary<Rabbit, RabbitHistory> aliveRabbits;
        public readonly List<Vector3> grassPositions;
        public readonly ConcurrentBag<RabbitHistory> rabbits;
        public int lifeTime = 0;
        public Vector2Int worldSize;

        public WorldHistory(MultiTypeEventHandler<HistoryEventType, float, int, Vector3> worldEvents)
        {
            if(Settings.World.collectHistory)
            {
                grassPositions = new List<Vector3>();
                aliveRabbits = new ConcurrentDictionary<Rabbit, RabbitHistory>();
                rabbits = new ConcurrentBag<RabbitHistory>();
                worldEvents.Subscribe(HistoryEventType.TURN_UPDATE, (object sender, int amount) => UpdateTurnEvent(amount));
                worldEvents.Subscribe(HistoryEventType.DEATH, (object sender, Vector3 position) => HandleDeathEvent(sender, position));
                worldEvents.Subscribe(HistoryEventType.BIRTH, (object sender, Vector3 position) => HandleBirthEvent(sender, position));
                worldEvents.Subscribe(HistoryEventType.EAT, (object sender, float amount) => HandleEatEvent(sender, amount));
                worldEvents.Subscribe(HistoryEventType.POSITION, (object sender, Vector3 position) => HandlePositionEvent(sender, position));
            }
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
            else throw new System.Exception("Unhandeled event sender");
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
            else throw new System.Exception("Unhandeled event sender");
        }

        private void HandleEatEvent(object obj, float amount)
        {
            if (typeof(Rabbit).IsInstanceOfType(obj))
            {
                RabbitEat(obj as Rabbit, amount);
            }
            else throw new System.Exception("Unhandeled event sender");
        }

        private void HandlePositionEvent(object obj, Vector3 position)
        {
            if (typeof(Rabbit).IsInstanceOfType(obj))
            {
                RabbitPosition(obj as Rabbit, position);
            }
            else throw new System.Exception("Unhandeled event sender");
        }

        private void RabbitDeath(Rabbit rabbit, Vector3 pos)
        {

            if (!aliveRabbits.ContainsKey(rabbit))
            {
                throw new System.Exception("Rabbit has not yet been born");
            }
            aliveRabbits.TryRemove(rabbit, out RabbitHistory hist);
            hist.deathPosition = pos;
            hist.lifeTime = lifeTime - hist.lifeTime;
            rabbits.Add(hist);
        }

        private void RabbitBirth(Rabbit rabbit, Vector3 pos)
        {
            if (aliveRabbits.ContainsKey(rabbit))
            {
                throw new System.Exception("Rabbit is a Jezus, he was reborned");
            }
            aliveRabbits.TryAdd(rabbit, new RabbitHistory { birthPosition = pos, deathPosition = Vector3.zero, foodEaten = 0f, lifeTime = lifeTime });
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

        private void RabbitPosition(Rabbit rabbit, Vector3 position)
        {
            if (!aliveRabbits.ContainsKey(rabbit))
            {
                throw new System.Exception("Rabbit has not yet been born");
            }
            var hist = aliveRabbits[rabbit];
            hist.positions.Add(position);
            aliveRabbits[rabbit] = hist;
        }

        private void AddGrass(Vector3 pos)
        {
            grassPositions.Add(pos);
        }

        public class RabbitHistory
        {
            public Vector3 birthPosition, deathPosition;
            public int lifeTime;
            public float foodEaten;
            public List<Vector3> positions = new List<Vector3>();
        }
    }
}