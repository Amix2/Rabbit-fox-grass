using UnityEngine;

namespace World
{
    public class Rabbit : Animal, IEdible
    {

        private int numOfSectors;
        private Grass closestGrass;
        private Grass[] closestGrassInSectors;
        private float[] netInputs;
        private float sqrRabbitEatingDistance;
        private float sqrAnimalViewRange;

        private Vector3 ViewForward => UseLocalViewSpace ? Forward : Vector3.forward;
        private float ViewAngleOffset => 180f / numOfSectors;

        protected override void ConsumeFood()
        {
            if (closestGrass != null)
            {
                float food = closestGrass.Consumed(Settings.Rabbit.rabbitEatingSpeed * Settings.World.simulationDeltaTime);
                Health += food;
                Health = Mathf.Clamp01(Health);
                world.WorldEvents.Invoke(this, HistoryEventType.EAT, food);
            }
        }

        // fill netInputs with suqre distance to nearest objects
        protected override void CollectInfoAboutSurroundings()
        {
            // clear data from previous runs
            float sqrDistanceToClosestGrass = sqrRabbitEatingDistance;
            closestGrass = null;
            for (int i = 0; i < 2 * numOfSectors; i++)    // clear data for grass and for foxes
            {
                netInputs[i] = sqrAnimalViewRange;
            }
;
            // iterate over all grass objects in the world, find closest one (for eatting) and closest in each sector
            foreach (Grass grass in world.grassList)
            {
                // dont touch grass with less than 0.1 health
                if (grass.Health > 0.1f)
                {
                    Vector3 grassPos = grass.Position;
                    Vector3 grassOffset = (grassPos - position);
                    float grassDist = grassOffset.sqrMagnitude;

                    // assign closest grass for eatting
                    if (grassDist < sqrDistanceToClosestGrass)
                    {
                        sqrDistanceToClosestGrass = grassDist;
                        closestGrass = grass;
                    }

                    // grass with health < 0.5 doesnt count as input value
                    if (grass.Health > 0.5f)
                    {
                        int sector = GetSector(grassOffset.normalized);

                        if (grassDist < netInputs[sector])
                        {
                            netInputs[sector] = grassDist;
                        }
                    }
                }
            }
        }

        private int GetSector(Vector3 grassOffset)
        {
            if (grassOffset.sqrMagnitude == 0f) return 0;
            Vector3 cross = Vector3.Cross(ViewForward, grassOffset);
            float angle = Vector3.Angle(ViewForward, grassOffset);
            angle += ViewAngleOffset;
            if (cross.y > 0.001f)
            {
                angle = 360f - angle;
                if (angle == 360f)
                {
                    angle = 0f;
                    Debug.LogWarning(cross.y);
                }
            }
            return (int)(angle * numOfSectors / 360f);
        }

        protected override float[] CreateNetInputs()
        {
            
            for (int i = 0; i < 2 * numOfSectors; i++)    // normalize data
            {
                netInputs[i] = sqrAnimalViewRange - netInputs[i];
                netInputs[i] /= sqrAnimalViewRange;
            }
            if(Settings.World.rabbitHungerInNeuralNet) netInputs[Settings.Rabbit.neuralNetworkLayers[0]-1] = Health;
            return netInputs;
        }

        public float FoodAmount
        {
            get { return Settings.Rabbit.foodInRabbits; }
        }

        protected override float MaxVelocity
        {
            get { return Settings.Rabbit.rabbitMaxVelocity; }
        }

        public override float HungerRate { get { return Settings.Rabbit.rabbitHungerRate; } }

        static bool firstInit = true;
        private new void Awake()
        {
            numOfSectors = Settings.Rabbit.neuralNetworkLayers[0] / 2;
            if (firstInit)
            {
                firstInit = false;
                Debug.LogFormat("Rabbit: sectors: {0}, net size: {1}, values filled by surroundings: {2}, hunger in net: {3}", numOfSectors, Settings.Rabbit.neuralNetworkLayers[0], 2 * numOfSectors, Settings.World.rabbitHungerInNeuralNet);
            }
            base.Awake();
            closestGrassInSectors = new Grass[numOfSectors];
            netInputs = new float[Settings.Rabbit.neuralNetworkLayers[0]];
            sqrRabbitEatingDistance = Settings.Rabbit.rabbitEatingDistance * Settings.Rabbit.rabbitEatingDistance;
            sqrAnimalViewRange = Settings.World.animalViewRange * Settings.World.animalViewRange;
        }

        public float Consumed(float amount = 1)
        {
            if (Health < 0.1f) return 0f;
            amount = Mathf.Min(amount, Health);
            Health = Mathf.Clamp01(Health - amount);
            return FoodAmount * amount;
        }

        private void OnDrawGizmosSelected()
        {
            float angle = 360f / numOfSectors;
            for (int i = 0; i < numOfSectors; i++)    
            {
                Gizmos.DrawLine(Position + transform.parent.position, Position + Quaternion.AngleAxis(i * angle + ViewAngleOffset, Vector3.up) * ViewForward * Settings.World.animalViewRange + transform.parent.position);
            }
            Gizmos.color = Color.red;
            foreach (Grass grass in closestGrassInSectors)
            {
                if (grass == null) continue;
                Gizmos.DrawLine(position + transform.parent.position, grass.Position + transform.parent.position);
            }
        }
    }
}