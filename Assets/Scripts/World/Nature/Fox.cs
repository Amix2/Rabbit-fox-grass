using UnityEngine;

namespace World
{
    public class Fox : Animal
    {
        private Rabbit closestRabbit;
        private Rabbit[] closestRabbitInSectors;
        private float[] netInputs;
        private float sqrFoxEatingDistance;
        private float sqrAnimalViewRange;

        protected override void ConsumeFood()
        {
            if (closestRabbit != null)
            {
                Vector3 rabbitPos = closestRabbit.Position;
                Vector3 rabbitOffset = (rabbitPos - position);
                float rabbitDist = rabbitOffset.sqrMagnitude;
                if (rabbitDist > sqrFoxEatingDistance)
                    return;
                float food = closestRabbit.Consumed(Settings.Fox.foxEatingSpeed * Settings.World.simulationDeltaTime);
                // Debug.Log("FOx eat: " + food + " Health: "+ Health) ;
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
            if (netInputs == null || netInputs.Length != SegmentCount + 1)
            {
                closestRabbitInSectors = new Rabbit[SegmentCount];
                netInputs = new float[SegmentCount + 1];
            }

            netInputs[0] = Health;

            for (int i = 1; i < SegmentCount + 1; i++)    // clear data for rabbit and for foxes
                netInputs[i] = sqrAnimalViewRange;

            closestRabbit = null;
            float sqrDistanceToClosestRabbit = float.MaxValue;// sqrFoxEatingDistance;
            // iterate over all rabbit objects in the world, find closest one (for eatting) and closest in each sector
            foreach (Animal animal in world.animalList)
            {
                Rabbit rabbit = animal as Rabbit;
                if (rabbit == null) continue;

                Vector3 rabbitPos = rabbit.Position;
                Vector3 rabbitOffset = (rabbitPos - position);
                float rabbitDist = rabbitOffset.sqrMagnitude;
                // assign closest rabbit for eatting
                if (rabbitDist < sqrDistanceToClosestRabbit)
                {
                    sqrDistanceToClosestRabbit = rabbitDist;
                    closestRabbit = rabbit;
                }
                // dont touch rabbit with less than 0.1 health
                if (rabbitDist < sqrAnimalViewRange && rabbit.Health > 0.1f)
                {
                    // rabbit with health dont count as input value
                    if (rabbit.Health > 0.2f)
                    {
                        int sector = GetSector(rabbitOffset.normalized) + 1;    // +1 cos [0] is health

                        if (netInputs[sector] == sqrAnimalViewRange)
                        {   // fist rabbit in this sector
                            netInputs[sector] = rabbitDist;
                        }
                        else
                        {   // multiple rabbit in this sector
                            float newDist = (netInputs[sector] + rabbitDist) * 0.5f * 0.5f; // half of the average
                            if (newDist < netInputs[sector])
                            {
                                netInputs[sector] = newDist;
                            }
                        }
                    }
                }
            }
        }

        public override Vector3 GetDecision() 
        {
            UseLocalViewSpace = false;
            if (closestRabbit == null)
                return new Vector3(0,0,0);

            Vector3 rabbitPos = closestRabbit.Position;
            Vector3 rabbitOffset = (rabbitPos - position);
            return rabbitOffset.normalized;
        }

        protected override float[] CreateNetInputs()
        {
            //for (int i = 0; i < numOfSectors; i++)    // normalize data
            //{
            //    netInputs[i] = sqrAnimalViewRange - netInputs[i];
            //    netInputs[i] /= sqrAnimalViewRange;
            //}
            //if (Settings.World.foxHungerInNeuralNet)
            //{
            //    int i = Settings.Fox.brainParams[0] - 1;
            //    if (i < 0 || i >= netInputs.Length)
            //    {
            //        int asd = 0;

            //    }
            //    netInputs[i] = Health;
            //}
            return netInputs;
        }

        protected override float MaxVelocity
        {
            get { return Settings.Fox.foxMaxVelocity; }
        }

        public override float HungerRate
        { get { return Settings.Fox.foxHungerRate; } }

        private new void Awake()
        {

            base.Awake();

            sqrFoxEatingDistance = Settings.Fox.foxEatingDistance * Settings.Fox.foxEatingDistance;
            sqrAnimalViewRange = Settings.World.animalViewRange * Settings.World.animalViewRange;
        }

        protected override void MultiplyAnimal()
        {
            var multiplyChance = Utils.RandomFloat();

            if (!(multiplyChance < Settings.Fox.foxMultiplicationChance)) return;
            world.AddFoxToMultiply(WorldCreator.prefabs["fox"], CalculateMultipliedAnimalPosition());
            Health -= Settings.Fox.healthDropAfterMultiplied;
        }

        public float Consumed(float amount = 1)
        {
            throw new System.NotImplementedException();
        }

        private void OnDrawGizmosSelected()
        {
            float angle = 360f / SegmentCount;
            for (int i = 0; i < SegmentCount; i++)
            {
                Gizmos.DrawLine(Position + transform.parent.position, Position + Quaternion.AngleAxis(i * angle + ViewAngleOffset, Vector3.up) * ViewForward * Settings.World.animalViewRange + transform.parent.position);
            }
            Gizmos.color = Color.red;
            foreach (Rabbit rabbit in closestRabbitInSectors)
            {
                if (rabbit == null) continue;
                Gizmos.DrawLine(position + transform.parent.position, rabbit.Position + transform.parent.position);
            }
        }
    }
}