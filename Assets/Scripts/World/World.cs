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

        private List<Rabbit> rabbitList;
        private List<Grass> grassList;

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
        }

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

        public bool UpdateTurn()
        {
            history.lifeTime++;

            float sqrViewRange = Settings.World.animalViewRange * Settings.World.animalViewRange;
            int initialListSize = Mathf.Min((int)Settings.World.animalViewRange, grassList.Count);

                Profiler.BeginSample("rabbit neighbours parallel");
            FillAllRabbitsPrayPredatorParallel(sqrViewRange, initialListSize);
                Profiler.EndSample();

                Profiler.BeginSample("rabbit nei normal");
            FillAllRabbitsPrayPredatorLinear(sqrViewRange, initialListSize);
                Profiler.EndSample();

            Profiler.BeginSample("rabbit linear");
            for (int i = 0; i < rabbitList.Count; i++)
            {
                var alive = rabbitList[i].UpdateTurn();
                if (!alive)
                {
                    history.RabbitDeath(rabbitList[i].Position);
                    Destroy(rabbitList[i].gameObject);
                    rabbitList.RemoveAt(i);
                    i--;
                }
            }
            Profiler.EndSample();

            //Profiler.BeginSample("rabbit parallel");
            //Parallel.ForEach(rabbitList, rabbit =>
            //{
            //    rabbit.UpdateTurn();
            //});

            //Profiler.EndSample();
            //foreach(Rabbit rabbit in rabbitList)
            //{
            //    if (!rabbit.IsAlive)
            //    {
            //        history.RabbitDeath(rabbit.transform.position);
            //        Destroy(rabbit.gameObject);
            //    }
            //}
            //rabbitList.RemoveAll(rabbit => !rabbit.IsAlive);



            foreach (var grass in grassList)
            {
                grass.UpdateTurn();
            }

            return rabbitList.Count > 0;

        }

        private void FillAllRabbitsPrayPredatorLinear(float sqrViewRange, int initialListSize)
        {
            foreach (Rabbit rabbit in rabbitList)
            {
                FillRabbitPrayPredator(rabbit, sqrViewRange, initialListSize);
            }
        }

        private void FillAllRabbitsPrayPredatorParallel(float sqrViewRange, int initialListSize)
        {
            Parallel.ForEach<Rabbit>(rabbitList, rabbit =>

            {
                FillRabbitPrayPredator(rabbit, sqrViewRange, initialListSize);
            }
            );
        }

        void FillRabbitPrayPredator(Rabbit rabbit, float sqrAnimalViewRange, int initialListSize)
        {
            rabbit.prayList = new List<INeuralNetInputProvider>(initialListSize);
            rabbit.predatorList = new List<INeuralNetInputProvider>(initialListSize);
            Vector3 position = rabbit.Position;
            foreach (Grass grass in grassList)
            {
                if ((grass.Position - position).sqrMagnitude < sqrAnimalViewRange)
                {
                    rabbit.prayList.Add(grass);
                }
            }
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
    }
}