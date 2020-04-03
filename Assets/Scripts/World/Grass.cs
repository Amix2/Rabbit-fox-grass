using UnityEngine;

namespace World
{
    public class Grass : MonoBehaviour, IEdible, IAlive, IUpdatable
    {
        public float FoodAmount
        {
            get { return Settings.World.foodInGrass; }
        }

        private float health = 1f;

        public float Health
        {
            get { return health; }
            set
            {
                health = value;
                modelTransform.localScale = new Vector3(health, health, health);
            }
        }

        public float HungerRate { get { return 0f; } }

        public bool IsAlive => Health > 0f;

        private Transform modelTransform = null;

        public float Consumed(float amount = 1)
        {
            if (!IsAlive) return 0f;
            Health = Mathf.Clamp01(Health - amount);
            return FoodAmount * amount;
        }

        public bool UpdateTurn()
        {
            Health += Settings.World.grassGrowthRate * Time.fixedDeltaTime;

            if (Health > 1f) Health = 1f;
            return true;
        }

        private void Awake()
        {
            foreach (Transform eachChild in transform)
            {
                if (eachChild.name == "Model")
                {
                    modelTransform = eachChild;
                }
            }
        }
    }
}