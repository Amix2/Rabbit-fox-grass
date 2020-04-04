using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace World
{
    public class World : MonoBehaviour
    {
        public Vector2Int size;

        private List<Rabbit> rabbitList;
        private List<Grass> grassList;

        private void Awake()
        {
            rabbitList = new List<Rabbit>();
            grassList = new List<Grass>();
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
            rabbitGO.GetComponent<Rabbit>().Brain = new NeuralNetwork(Settings.Player.neuralNetworkLayers);
            new Vector3(transform.position.x, transform.position.y, transform.position.z);
            rabbitList.Add(rabbitGO.GetComponent<Rabbit>());
        }

        public void AddGrass(GameObject prefab, Vector3 position)
        {
            var grassGO = AddGameObject(prefab, position);
            grassGO.name = "Grass_" + grassList.Count;
            grassList.Add(grassGO.GetComponent<Grass>());
        }

        private GameObject AddGameObject(GameObject prefab, Vector3 position)
        {
            var obj = Instantiate(prefab, transform);
            obj.transform.localPosition = position;
            return obj;
        }

        public void UpdateBehaviour()
        {
            if (IsAnyAnimalAlive())
            {
                foreach (var rabbit in rabbitList)
                {
                    if (rabbit.IsAlive)
                        rabbit.UpdateBehaviour();
                }
            }
            else
            {
                Debug.Log("All animals are dead");
            }
        }

        private bool IsAnyAnimalAlive()
        {
            return rabbitList.Any(rabbit => rabbit.IsAlive);
        }
    }
}