using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class WorldHistory
    {
        private readonly ConcurrentDictionary<Animal, AnimalHistory> aliveRabbits;
        public readonly ConcurrentBag<AnimalHistory> rabbits;
        private readonly ConcurrentDictionary<Animal, AnimalHistory> aliveFoxes;
        public readonly ConcurrentBag<AnimalHistory> foxes;
        public readonly List<Vector3> grassPositions;
        public Vector2Int worldSize;

        public int LifeTime { get; private set; } = 0;

        public WorldHistory(MultiTypeEventHandler<HistoryEventType, float, int, Vector3> worldEvents)
        {
            if(Settings.World.collectHistory)
            {
                grassPositions = new List<Vector3>();
                aliveRabbits = new ConcurrentDictionary<Animal, AnimalHistory>();
                rabbits = new ConcurrentBag<AnimalHistory>();
                aliveFoxes = new ConcurrentDictionary<Animal, AnimalHistory>();
                foxes = new ConcurrentBag<AnimalHistory>();
                worldEvents.Subscribe(HistoryEventType.TURN_UPDATE, (object sender, int amount) => UpdateTurnEvent(amount));
                worldEvents.Subscribe(HistoryEventType.DEATH, (object sender, Vector3 position) => HandleDeathEvent(sender, position));
                worldEvents.Subscribe(HistoryEventType.BIRTH, (object sender, Vector3 position) => HandleBirthEvent(sender, position));
                worldEvents.Subscribe(HistoryEventType.EAT, (object sender, float amount) => HandleEatEvent(sender, amount));
                worldEvents.Subscribe(HistoryEventType.POSITION, (object sender, Vector3 position) => HandlePositionEvent(sender, position));
            }
        }

        private void UpdateTurnEvent(int amount)
        {
            LifeTime += amount;
        }

        private void HandleDeathEvent(object obj, Vector3 position)
        {
            if (typeof(Grass).IsInstanceOfType(obj))
            {
                AddGrass(position);
            }
            else if (typeof(Rabbit).IsInstanceOfType(obj))
            {
                AnimalDeath(aliveRabbits, rabbits, obj as Rabbit, position);
            }
            else if (typeof(Fox).IsInstanceOfType(obj))
            {
                AnimalDeath(aliveFoxes, foxes, obj as Fox, position);
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
                AnimalBirth(aliveRabbits, obj as Rabbit, position);
            }
            else if (typeof(Fox).IsInstanceOfType(obj))
            {
                AnimalBirth(aliveFoxes, obj as Fox, position);
            }
            else throw new System.Exception("Unhandeled event sender");
        }

        private void HandleEatEvent(object obj, float amount)
        {
            if (typeof(Rabbit).IsInstanceOfType(obj))
            {
                AnimalEat(aliveRabbits, obj as Rabbit, amount);
            }
            else if (typeof(Fox).IsInstanceOfType(obj))
            {
                AnimalEat(aliveFoxes, obj as Fox, amount);
            }
            else throw new System.Exception("Unhandeled event sender");
        }

        private void HandlePositionEvent(object obj, Vector3 position)
        {
            if (typeof(Rabbit).IsInstanceOfType(obj))
            {
                AnimalPosition(aliveRabbits, obj as Rabbit, position);
            }
            else if (typeof(Fox).IsInstanceOfType(obj))
            {
                AnimalPosition(aliveFoxes, obj as Fox, position);
            }
            else throw new System.Exception("Unhandeled event sender : " + obj.GetType());
        }

        private void AnimalDeath(ConcurrentDictionary<Animal, AnimalHistory>  aliveAnimals, ConcurrentBag<AnimalHistory> deadAnimals, Animal animal, Vector3 pos)
        {

            if (!aliveAnimals.ContainsKey(animal))
            {
                throw new System.Exception("Animal has not yet been born");
            }
            aliveAnimals.TryRemove(animal, out AnimalHistory hist);
            hist.DeathTime = LifeTime;
            deadAnimals.Add(hist);
        }

        private void AnimalBirth(ConcurrentDictionary<Animal, AnimalHistory> aliveAnimals, Animal animal, Vector3 pos)
        {
            if (aliveAnimals.ContainsKey(animal))
            {
                throw new System.Exception("Animal is a Jezus, he was reborned");
            }
            aliveAnimals.TryAdd(animal, new AnimalHistory { BirthTime = LifeTime });
        }

        private void AnimalEat(ConcurrentDictionary<Animal, AnimalHistory> aliveAnimals, Animal animal, float food)
        {
            if (!aliveAnimals.ContainsKey(animal))
            {
                throw new System.Exception("Rabbit has not yet been born");
            }
            var hist = aliveAnimals[animal];
            hist.FoodEaten += food;
            aliveAnimals[animal] = hist;
        }

        private void AnimalPosition(ConcurrentDictionary<Animal, AnimalHistory> aliveAnimals, Animal animal, Vector3 position)
        {
            if (!aliveAnimals.ContainsKey(animal))
            {
                throw new System.Exception("Rabbit has not yet been born");
            }
            var hist = aliveAnimals[animal];
            hist.Positions.Add(position);
            aliveAnimals[animal] = hist;
        }

        private void AddGrass(Vector3 pos)
        {
            grassPositions.Add(pos);
        }

    }
}