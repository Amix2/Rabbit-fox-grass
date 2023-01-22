using UnityEngine;

namespace World
{
    public class Rabbit : Animal, IEdible
    {
        private Grass closestGrass;
        private Grass[] closestGrassInSectors;
        private float[] netInputs;
        private float[] sectorGrassDistances;
        private float[] sectorFoxDistances;
        private float sqrRabbitEatingDistance;
        private float sqrAnimalViewRange;


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
            if (netInputs == null || netInputs.Length != 2*SegmentCount + 1)
            {
                closestGrassInSectors = new Grass[SegmentCount];
                netInputs = new float[SegmentCount + SegmentCount + 1];
                sectorGrassDistances = new float[SegmentCount];
                sectorFoxDistances = new float[SegmentCount];
            }

            netInputs[0] = Health;


            // clear data from previous runs
            float sqrDistanceToClosestGrass = sqrRabbitEatingDistance;
            closestGrass = null;
            for (int i = 0; i < SegmentCount; i++)    // clear data for grass and for foxes
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
                if(foxDist < sqrAnimalViewRange)
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
        protected override float[] CreateNetInputs()
        {
            netInputs[0] = Health;
            for (int i = 0; i < SegmentCount; i++)    // normalize data
            {
                int netID = i + 1;
                netInputs[netID] = sqrAnimalViewRange - sectorGrassDistances[i];
                netInputs[netID] /= sqrAnimalViewRange;
            }

            for (int i = 0; i < SegmentCount; i++)    // normalize data
            {
                int netID = i + 1 + SegmentCount;
                netInputs[netID] = sqrAnimalViewRange - sectorFoxDistances[i];
                netInputs[netID] /= sqrAnimalViewRange;
            }
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

        private new void Awake()
        {
            base.Awake();
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
            if (Health <= 0f)
            {
                HandleDeath();
            }
            return FoodAmount * amount;
        }

        private void OnDrawGizmosSelected()
        {
            CollectInfoAboutSurroundings();
            float angle = 360f / SegmentCount;
            Gizmos.color = Color.white;
            for (int i = 0; i < SegmentCount; i++)
            {
                Gizmos.DrawLine(Position + transform.parent.position, Position + Quaternion.AngleAxis(i * angle + ViewAngleOffset, Vector3.up) * ViewForward * Settings.World.animalViewRange + transform.parent.position);
            }
            Gizmos.color = Color.green;
            for (int i = 0; i < SegmentCount; i++)
            {
                Vector3 offset = Quaternion.AngleAxis((i + 0.48f) * angle + ViewAngleOffset, Vector3.up) * ViewForward;
                Gizmos.DrawLine(Position + transform.parent.position, Position + offset * Mathf.Sqrt(sectorGrassDistances[GetSector(offset)]) + transform.parent.position);
            }

            Gizmos.color = Color.red;
            for (int i = 0; i < SegmentCount; i++)
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