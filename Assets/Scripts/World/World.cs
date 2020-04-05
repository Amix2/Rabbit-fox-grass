using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace World
{
    public class World : MonoBehaviour, IUpdatable
    {
        public IBigBrain bigBrain;
        public Vector2Int size;

        public static float deltaTime;

        public List<Rabbit> rabbitList;
        public List<Grass> grassList;

        private ConcurrentBag<Rabbit> deadRabbits;

        private WorldHistory history;
        public WorldHistory History
        {
            get { return history; }
        }

        public bool IsAlive { get => rabbitList.Count > 0; }

        private void Awake()
        {
            rabbitList = new List<Rabbit>();
            grassList = new List<Grass>();
            history = new WorldHistory();
            deadRabbits = new ConcurrentBag<Rabbit>();
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
            if (rabbitList.Count == deadRabbits.Count) return false;

            while(deadRabbits.TryTake(out Rabbit deadRabbit))
            {
                history.RabbitDeath(deadRabbit.Position);
                Destroy(deadRabbit.gameObject);
                rabbitList.Remove(deadRabbit);
            }

            return true;

        }

        private void Start()
        {
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

        //////////////////////////////////////////
        /// Setup

        public void Apply()
        {
            Transform planeTransform = transform.GetChild(0);
            planeTransform.Translate(new Vector3(size.x * 0.5f, 0f, size.y * 0.5f));
            planeTransform.localScale = new Vector3(size.x, 0.1f, size.y);
        }

        public void AddRabbit(GameObject prefab, Vector3 position)
        {
            var rabbitGO = AddGameObject(prefab, position);
            rabbitGO.name = "Rabbit_" + rabbitList.Count;
            rabbitGO.GetComponent<Rabbit>().worldSize = size;
            rabbitGO.GetComponent<Rabbit>().Brain = bigBrain;
            rabbitList.Add(rabbitGO.GetComponent<Rabbit>());
            history.RabbitBirth(position);
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