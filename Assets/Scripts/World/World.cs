using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class World : MonoBehaviour, IUpdatable
    {
        public IBigBrain bigBrain;
        public Vector2Int size;


        private List<Rabbit> rabbitList;
        private List<Grass> grassList;
        private List<Rabbit> aliveRabbitList;

        private WorldHistory history;
        public WorldHistory History
        {
            get { return history; }
        }

        private void Awake()
        {
            rabbitList = new List<Rabbit>();
            grassList = new List<Grass>();
            aliveRabbitList = new List<Rabbit>();
            history = new WorldHistory();
        }

        public void ResetWorld()
        {
            aliveRabbitList.AddRange(rabbitList);
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
            for (int i=0; i<rabbitList.Count; i++)
            {
                var alive = rabbitList[i].UpdateTurn();
                if(!alive)
                {
                    history.RabbitDeath(rabbitList[i].transform.position);
                    Destroy(rabbitList[i].gameObject);
                    rabbitList.RemoveAt(i);
                    i--;
                }
            }

            foreach (var grass in grassList)
            {
                grass.UpdateTurn();
            }

            return rabbitList.Count > 0;
        }
    }
}