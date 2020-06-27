using UnityEngine;

namespace World
{
    public class Rabbit : Animal, IEdible
    {
        private int numOfSectors;
        private Grass closestGrass;
        private Grass[] closestGrassInSectors;
        private float[] netInputs;
        private float[] sectorGrassDistances;
        private float[] sectorFoxDistances;
        private float sqrRabbitEatingDistance;
        private float sqrAnimalViewRange;

        private Vector3 ViewForward => UseLocalViewSpace ? Forward : Vector3.forward;
        private float ViewAngleOffset => -180f / numOfSectors;

        protected override void ConsumeFood()
        {
            if (closestGrass != null)
            {
                float food = closestGrass.Consumed(Settings.Rabbit.rabbitEatingSpeed * Settings.World.simulationDeltaTime);
                Health += food;
                Health = Mathf.Clamp01(Health);
                world.WorldEvents.Invoke(this, HistoryEventType.EAT, food);

                // Multiply after consuming if health is full
                if (Health > 0.99f)
                    MultiplyAnimal();
            }
        }

        // fill netInputs with suqre distance to nearest objects
        protected override void CollectInfoAboutSurroundings()
        {
            // clear data from previous runs
            float sqrDistanceToClosestGrass = sqrRabbitEatingDistance;
            closestGrass = null;
            for (int i = 0; i < numOfSectors; i++)    // clear data for grass and for foxes
            {
                sectorGrassDistances[i] = sqrAnimalViewRange;
                sectorFoxDistances[i] = sqrAnimalViewRange;
            }
            // iterate over all grass objects in the world, find closest one (for eatting) and closest in each sector
            foreach (Grass grass in world.grassList)
            {
                Vector3 grassPos = grass.Position;
                Vector3 grassOffset = (grassPos - position);
                float grassDist = grassOffset.sqrMagnitude;
                // dont touch grass with less than 0.1 health
                if (grassDist < sqrAnimalViewRange && grass.Health > 0.1f)
                {

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

                        if (sectorGrassDistances[sector] == sqrAnimalViewRange)
                        {   // fist grass in this sector
                            sectorGrassDistances[sector] = grassDist;
                        }
                        else
                        {   // multiple grass in this sector
                            float newDist = (sectorGrassDistances[sector] + grassDist) * 0.5f * 0.5f; // half of the average
                            if (newDist < sectorGrassDistances[sector])
                            {
                                sectorGrassDistances[sector] = newDist;
                            }
                        }
                    }
                }
            }

            foreach (Animal animal in world.animalList)
            {
                Fox fox = animal as Fox;
                if (fox == null) continue;

                Vector3 foxPos = fox.Position;
                Vector3 foxOffset = (foxPos - position);
                float foxDist = foxOffset.sqrMagnitude;
                if(foxDist > sqrAnimalViewRange)
                {
                    int sector = GetSector(foxOffset.normalized);

                    if (sectorFoxDistances[sector] == sqrAnimalViewRange)
                    {   // fist fox in this sector
                        sectorFoxDistances[sector] = foxDist;
                    }
                    else
                    {   // multiple fox in this sector
                        float newDist = (sectorFoxDistances[sector] + foxDist) * 0.5f * 0.5f; // half of the average
                        if (newDist < sectorFoxDistances[sector])
                        {
                            sectorFoxDistances[sector] = newDist;
                        }
                    }
                }
            }
        }

        private int GetSector(Vector3 grassOffset)
        {
            if (grassOffset.sqrMagnitude == 0f) return 0;
            grassOffset = Quaternion.AngleAxis(ViewAngleOffset, Vector3.up) * grassOffset.normalized;
            Vector3 cross = Vector3.Cross(ViewForward, grassOffset);
            float angle = Vector3.Angle(ViewForward, grassOffset);
            //angle += ViewAngleOffset;
            if (cross.y > 0.001f)   // angle more than 180
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
            for (int i = 0; i < numOfSectors; i++)    // normalize data
            {
                netInputs[i] = sqrAnimalViewRange - sectorGrassDistances[i];
                netInputs[i] /= sqrAnimalViewRange;
            }

            for (int i = 0; i < numOfSectors; i++)    // normalize data
            {
                netInputs[numOfSectors + i] = sqrAnimalViewRange - sectorFoxDistances[i];
                netInputs[numOfSectors + i] /= sqrAnimalViewRange;
            }
            if (Settings.World.rabbitHungerInNeuralNet) netInputs[Settings.Rabbit.neuralNetworkLayers[0] - 1] = Health;
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

        private static bool firstInit = true;

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
            sectorGrassDistances = new float[numOfSectors];
            sectorFoxDistances = new float[numOfSectors];
            sqrRabbitEatingDistance = Settings.Rabbit.rabbitEatingDistance * Settings.Rabbit.rabbitEatingDistance;
            sqrAnimalViewRange = Settings.World.animalViewRange * Settings.World.animalViewRange;
        }

        protected override void MultiplyAnimal()
        {
            var multiplyChance = Utils.RandomFloat();

            if (!(multiplyChance < Settings.Rabbit.rabbitMultiplicationChance)) return;
            world.AddRabbitToMultiply(WorldCreator.prefabs["rabbit"], CalculateMultipliedAnimalPosition());
            Health -= Settings.Rabbit.healthDropAfterMultiplied;
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
            CollectInfoAboutSurroundings();
            float angle = 360f / numOfSectors;
            Gizmos.color = Color.white;
            for (int i = 0; i < numOfSectors; i++)
            {
                Gizmos.DrawLine(Position + transform.parent.position, Position + Quaternion.AngleAxis(i * angle + ViewAngleOffset, Vector3.up) * ViewForward * Settings.World.animalViewRange + transform.parent.position);
            }
            Gizmos.color = Color.green;
            for (int i = 0; i < numOfSectors; i++)
            {
                Vector3 offset = Quaternion.AngleAxis((i + 0.48f) * angle + ViewAngleOffset, Vector3.up) * ViewForward;
                Gizmos.DrawLine(Position + transform.parent.position, Position + offset * Mathf.Sqrt(sectorGrassDistances[GetSector(offset)]) + transform.parent.position);
            }

            Gizmos.color = Color.red;
            for (int i = 0; i < numOfSectors; i++)
            {
                Vector3 offset = Quaternion.AngleAxis((i + 0.52f) * angle + ViewAngleOffset, Vector3.up) * ViewForward;
                Gizmos.DrawLine(Position + transform.parent.position, Position + offset * Mathf.Sqrt(sectorFoxDistances[GetSector(offset)]) + transform.parent.position);
            }

            //Gizmos.color = Color.blue;
            //for (int i = 0; i < numOfSectors; i++)
            //{
            //    Vector3 offset = Quaternion.AngleAxis((i + 0.52f) * angle + ViewAngleOffset, Vector3.up) * ViewForward;
            //    if(sectorGrassDistances[GetSector(offset)] < sqrAnimalViewRange)
            //    Gizmos.DrawLine(Position + transform.parent.position, closestGrassInSectors[GetSector(offset)].Position + transform.parent.position);
            //}

            //Gizmos.color = Color.cyan;
            //for (int i = 0; i < 360; i++)
            //{
            //    Vector3 offset = Quaternion.AngleAxis(i + ViewAngleOffset, Vector3.up) * ViewForward;
            //    Gizmos.DrawLine(Position + transform.parent.position, Position + offset.normalized * GetSector(offset) + transform.parent.position);
            //}
        }
    }
}