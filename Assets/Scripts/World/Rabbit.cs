using UnityEngine;
namespace World
{

    public class Rabbit : Animal, IEdible
    {
        private Collider[] grassColliders;

        public float Consumed(float amount = 1)
        {
            throw new System.NotImplementedException();
        }

        protected override void ConsumeFood()
        {
            int numOfCollidersFound = Physics.OverlapSphereNonAlloc(transform.position, 0.1f, grassColliders, feedOnLayer);
            GameObject closestGrassGO = null;

            for (int i = 0; i < numOfCollidersFound; i++)
            {
                if (grassColliders[i].gameObject.transform == transform || grassColliders[i].gameObject.transform.parent != transform.parent) continue;
                closestGrassGO = grassColliders[i].gameObject;
            }

            if (closestGrassGO != null)
            {
                float food = closestGrassGO.GetComponent<Grass>().Consumed(Settings.World.rabbitEatingSpeed * Time.fixedDeltaTime);
                Health += food;
            }
        }

        private void Start()
        {
            grassColliders = new Collider[9];
        }

        public float FoodAmount
        {
            get { return Settings.World.foodInRabbits; }
        }

        protected override float MaxVelocity
        {
            get { return Settings.World.rabbitMaxVelocity; }
        }

        public override float HungerRate { get { return Settings.World.rabbitHungerRate; } }
    }
}