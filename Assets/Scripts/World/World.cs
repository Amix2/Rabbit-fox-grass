using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class World : WorldObject, IUpdatable
    {
        private Vector2Int size;

        public static float deltaTime;

        public List<Animal> animalList;
        public List<Grass> grassList;

        private ConcurrentBag<Animal> deadAnimals;
        private ConcurrentBag<Tuple<GameObject, Vector3>> multiplyRabbitsQueue;
        private ConcurrentBag<Tuple<GameObject, Vector3>> multiplyFoxesQueue;

        public bool IsAlive { get => animalList.Count > 0; }

        public Vector2Int Size
        {
            get => size; set
            {
                size = value;
                History.worldSize = size;
            }
        }

        public WorldHistory History { get; private set; }
        private bool render;
        public bool Render { get
            {
                return render;
            }
            set
            {
                render = value;
                if(!value) DisableModel();
            }
        }
        public MultiTypeEventHandler<HistoryEventType, float, int, Vector3> WorldEvents { get; private set; }
        internal NeuralNetwork BigBrain { get; set; }
        internal NeuralNetwork FoxBrain { get; set; }

        private void Awake()
        {
            WorldEvents = new MultiTypeEventHandler<HistoryEventType, float, int, Vector3>();
            WorldEvents.Subscribe(HistoryEventType.DEATH, (object sender, Vector3 posiiton) => HandleDeath(sender));
            animalList = new List<Animal>();
            grassList = new List<Grass>();
            History = new WorldHistory(WorldEvents);
            deadAnimals = new ConcurrentBag<Animal>();
            multiplyRabbitsQueue = new ConcurrentBag<Tuple<GameObject, Vector3>>();
            multiplyFoxesQueue = new ConcurrentBag<Tuple<GameObject, Vector3>>();
        }

        public void HandleDeath(object obj)
        {
            if (typeof(Animal).IsInstanceOfType(obj)) deadAnimals.Add(obj as Animal);
        }

        /// <summary>
        /// Destroy dead animals
        /// </summary>
        /// <returns></returns>
        public bool UpdateTurn()
        {
            WorldEvents.Invoke(this, HistoryEventType.TURN_UPDATE, 1);
            while (deadAnimals.TryTake(out Animal deadAnimal))
            {
                Destroy(deadAnimal.gameObject);
                animalList.Remove(deadAnimal);
            }
            MultiplyAnimals();
            return IsAlive;
        }

        //////////////////////////////////////////
        /// Setup
        public void Apply()
        {
            Transform planeTransform = transform.GetChild(0);
            planeTransform.Translate(new Vector3(Size.x * 0.5f, 0f, Size.y * 0.5f));
            planeTransform.localScale = new Vector3(Size.x, 0.1f, Size.y);
        }

        public void AddRabbit(GameObject prefab, Vector3 position)
        {
            var rabbitGO = AddGameObject(prefab, position);
            rabbitGO.name = "Rabbit_" + animalList.Count;
            rabbitGO.GetComponent<Rabbit>().worldSize = Size;
            rabbitGO.GetComponent<Rabbit>().Brain = BigBrain;
            if (!Render) rabbitGO.GetComponent<Rabbit>().DisableModel();
            animalList.Add(rabbitGO.GetComponent<Rabbit>());
            rabbitGO.GetComponent<Rabbit>().world = this;
            WorldEvents.Invoke(rabbitGO.GetComponent<Rabbit>(), HistoryEventType.BIRTH, position);
        }

        public void AddGrass(GameObject prefab, Vector3 position)
        {
            var grassGO = AddGameObject(prefab, position);
            grassGO.name = "Grass_" + grassList.Count;
            if (!Render) grassGO.GetComponent<Grass>().DisableModel();
            grassList.Add(grassGO.GetComponent<Grass>());
            WorldEvents.Invoke(grassGO.GetComponent<Grass>(), HistoryEventType.BIRTH, position);
        }

        internal void AddFox(GameObject prefab, Vector3 position)
        {
            var foxGO = AddGameObject(prefab, position);
            foxGO.name = "Fox_" + animalList.Count;
            foxGO.GetComponent<Fox>().worldSize = Size;
            foxGO.GetComponent<Fox>().Brain = FoxBrain;
            if (!Render) foxGO.GetComponent<Fox>().DisableModel();
            animalList.Add(foxGO.GetComponent<Fox>());
            WorldEvents.Invoke(foxGO.GetComponent<Fox>(), HistoryEventType.BIRTH, position);
        }

        private GameObject AddGameObject(GameObject prefab, Vector3 position)
        {
            var obj = Instantiate(prefab, transform);
            obj.transform.localPosition = position;
            obj.GetComponent<WorldObject>().SetupObject();
            return obj;
        }

        public void AddRabbitToMultiply(GameObject gameObject, Vector3 position)
        {
            multiplyRabbitsQueue.Add(Tuple.Create(gameObject,position));
        }
        
        public void AddFoxToMultiply(GameObject gameObject, Vector3 position)
        {
            multiplyFoxesQueue.Add(Tuple.Create(gameObject,position));
        }

        private void MultiplyAnimals()
        {
            while (multiplyRabbitsQueue.TryTake(out Tuple<GameObject, Vector3> animalTuple))
            {
                AddRabbit(animalTuple.Item1,animalTuple.Item2);
            }
            
            while (multiplyFoxesQueue.TryTake(out Tuple<GameObject, Vector3> animalTuple))
            {
                AddFox(animalTuple.Item1,animalTuple.Item2);
            }
        }
    }
}