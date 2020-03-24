using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class World : MonoBehaviour
    {
        public IBigBrain bigBrain;
        public Vector2Int size;

        private List<Rabbit> rabbits;

        private void Awake()
        {
            rabbits = new List<Rabbit>();
        }

        public void Apply()
        {
            Transform planeTransform = transform.GetChild(0);
            planeTransform.position = new Vector3(size.x * 0.5f, 0f, size.y * 0.5f);
            planeTransform.localScale = new Vector3(size.x, 0.1f, size.y);
        }

        public void AddRabbit(GameObject prefab, Vector3 position)
        {
            var rabbitGO = Instantiate(prefab, position, Quaternion.identity);
            rabbitGO.transform.parent = transform;
            rabbits.Add(rabbitGO.GetComponent<Rabbit>());
        }


        public void UpdateBehaviour()
        {
        
        }
    }
}
