using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class World : MonoBehaviour, IUpdatable
    {
        private Vector2Int size;

        public static float deltaTime;

        public List<Rabbit> rabbitList;
        public List<Grass> grassList;

        private ConcurrentBag<Rabbit> deadRabbits;

        private WorldHistory history;
        internal IBigBrain bigBrain;

        public WorldHistory History
        {
            get { return history; }
        }

        public bool IsAlive { get => rabbitList.Count > 0; }

        public Vector2Int Size
        {
            get => size; set
            {
                size = value;
                history.worldSize = size;
            }
        }

        private void Awake()
        {
            rabbitList = new List<Rabbit>();
            grassList = new List<Grass>();
            history = new WorldHistory();
            deadRabbits = new ConcurrentBag<Rabbit>();
            if (Settings.Player.renderOptions == RenderOptions.None)
            {
                foreach (Transform eachChild in transform)
                {
                    if (eachChild.name == "Model")
                    {
                        eachChild.gameObject.SetActive(false);
                    }
                }
            }
        }

        public void HandleDeath(Rabbit rabbit)
        {
            deadRabbits.Add(rabbit);
        }

        /// <summary>
        /// Destroy dead animals
        /// </summary>
        /// <returns></returns>
        public bool UpdateTurn()
        {
            history.lifeTime++;

            while (deadRabbits.TryTake(out Rabbit deadRabbit))
            {
                history.RabbitDeath(deadRabbit, deadRabbit.Position);
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
            rabbitGO.GetComponent<Rabbit>().Brain = new NeuralNetwork(Settings.Player.neuralNetworkLayers);
            rabbitList.Add(rabbitGO.GetComponent<Rabbit>());
            rabbitGO.GetComponent<Rabbit>().world = this;
            history.RabbitBirth(rabbitGO.GetComponent<Rabbit>(), position);
        }

        public void AddGrass(GameObject prefab, Vector3 position)
        {
            var grassGO = AddGameObject(prefab, position);
            grassGO.name = "Grass_" + grassList.Count;
            grassList.Add(grassGO.GetComponent<Grass>());
            history.Grass(position);
        }

        private GameObject AddGameObject(GameObject prefab, Vector3 position)
        {
            var obj = Instantiate(prefab, transform);
            obj.transform.localPosition = position;
            return obj;
        }
    }
}