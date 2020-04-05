using UnityEngine;

namespace World
{
    public class Grass : WorldObject, IEdible, IAlive, IUpdatable
    {
        public float FoodAmount
        {
            get { return Settings.World.foodInGrass; }
        }

        private bool healthChange = false;
        private float health;
        public float Health
        {
            get { return health; }
            set
            {
                health = value;
                healthChange = true;
            }
        }

        public Color deadColor;
        public Color middleColor;
        public Color fullColor;

        public float HungerRate { get { return 0f; } }

        public bool IsAlive => Health > 0f;

        private Transform modelTransform = null;
        private MeshRenderer[] meshRenderers;

        public float Consumed(float amount = 1)
        {
            if (!IsAlive) return 0f;
            Health = Mathf.Clamp01(Health - amount);
            return FoodAmount * amount;
        }

        public bool UpdateTurn()
        {
            Health += Settings.World.grassGrowthRate * Settings.World.simulationDeltaTime;

            if (Health > 1f) Health = 1f;
            return true;
        }

        private new void Start()
        {
            foreach (Transform eachChild in transform)
            {
                if (eachChild.name == "Model")
                {
                    modelTransform = eachChild;
                }
            }
            base.Start();
            position = transform.position;
            meshRenderers = modelTransform.GetComponentsInChildren<MeshRenderer>();
            Health = 1f;
        }

        private void Update()
        {
            if(Settings.Player.renderOptions == RenderOptions.None || !healthChange) return;
            healthChange = false;
            modelTransform.localScale = new Vector3(health, health, health);
            foreach (var mesh in meshRenderers)
            {
                if (health < 0.5f)
                {
                    mesh.material.color = Color.Lerp(deadColor, middleColor, health * 2);
                }
                else

                {
                    mesh.material.color = Color.Lerp(middleColor, fullColor, (health - 0.5f) * 2);
                }
            }
        }
    }
}