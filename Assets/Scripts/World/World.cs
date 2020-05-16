using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class World : WorldObject, IUpdatable
    {
        private Vector2Int size;

        public static float deltaTime;

        public List<Rabbit> rabbitList;
        public List<Grass> grassList;

        private ConcurrentBag<Rabbit> deadRabbits;
        internal NeuralNetwork bigBrain;

        public bool IsAlive { get => rabbitList.Count > 0; }

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

        private new void Awake()
        {
            base.Awake();
            WorldEvents = new MultiTypeEventHandler<HistoryEventType, float, int, Vector3>();
            WorldEvents.Subscribe(HistoryEventType.DEATH, (object sender, Vector3 posiiton) => HandleDeath(sender));
            rabbitList = new List<Rabbit>();
            grassList = new List<Grass>();
            History = new WorldHistory(WorldEvents);
            deadRabbits = new ConcurrentBag<Rabbit>();
        }

        public void HandleDeath(object obj)
        {
            if (typeof(Rabbit).IsInstanceOfType(obj)) deadRabbits.Add(obj as Rabbit);
        }

        /// <summary>
        /// Destroy dead animals
        /// </summary>
        /// <returns></returns>
        public bool UpdateTurn()
        {
            History.lifeTime++;
            WorldEvents.Invoke(this, HistoryEventType.TURN_UPDATE, 1);
            while (deadRabbits.TryTake(out Rabbit deadRabbit))
            {
                Destroy(deadRabbit.gameObject);
                rabbitList.Remove(deadRabbit);
            }

            return rabbitList.Count > 0;
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
            rabbitGO.name = "Rabbit_" + rabbitList.Count;
            rabbitGO.GetComponent<Rabbit>().worldSize = Size;
            rabbitGO.GetComponent<Rabbit>().Brain = bigBrain;
            if (!Render) rabbitGO.GetComponent<Rabbit>().DisableModel();
            rabbitList.Add(rabbitGO.GetComponent<Rabbit>());
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

        private GameObject AddGameObject(GameObject prefab, Vector3 position)
        {
            var obj = Instantiate(prefab, transform);
            obj.transform.localPosition = position;
            return obj;
        }
    }
}