using UnityEngine;

namespace World
{
    public class Fox : Animal
    {

        private int numOfSectors;
        private Rabbit closestRabbit;
        private Rabbit[] closestRabbitInSectors;
        private float[] netInputs;
        private float sqrFoxEatingDistance;
        private float sqrAnimalViewRange;

        private Vector3 ViewForward => UseLocalViewSpace ? Forward : Vector3.forward;
        private float ViewAngleOffset => 180f / numOfSectors;

        protected override void ConsumeFood()
        {
            if (closestRabbit != null)
            {
                float food = closestRabbit.Consumed(Settings.Fox.foxEatingSpeed * Settings.World.simulationDeltaTime);
               // Debug.Log("FOx eat: " + food + " Health: "+ Health) ;
                Health += food;
                Health = Mathf.Clamp01(Health);
                world.WorldEvents.Invoke(this, HistoryEventType.EAT, food);
                
                // Multiply after consuming if health is full
                if(Health > 0.99f)
                    MultiplyAnimal();
            }
        }

        // fill netInputs with suqre distance to nearest objects
        protected override void CollectInfoAboutSurroundings()
        {
            // clear data from previous runs
            float sqrDistanceToClosestGrass = sqrFoxEatingDistance;
            closestRabbit = null;
            for (int i = 0; i < numOfSectors; i++)    // clear data for rabbit and for foxes
            {
                netInputs[i] = sqrAnimalViewRange;
            }
;
            // iterate over all rabbit objects in the world, find closest one (for eatting) and closest in each sector
            foreach (Animal animal in world.animalList)
            {
                Rabbit rabbit = animal as Rabbit;
                if (rabbit == null) continue;
                // dont touch rabbit with less than 0.1 health
                if (rabbit.Health > 0.1f)
                {
                    Vector3 rabbitPos = rabbit.Position;
                    Vector3 rabbitOffset = (rabbitPos - position);
                    float rabbitDist = rabbitOffset.sqrMagnitude;

                    // assign closest rabbit for eatting
                    if (rabbitDist < sqrDistanceToClosestGrass)
                    {
                        sqrDistanceToClosestGrass = rabbitDist;
                        closestRabbit = rabbit;
                    }

                    // rabbit with health < 0.5 doesnt count as input value
                    if (rabbit.Health > 0.2f)
                    {
                        int sector = GetSector(rabbitOffset.normalized);

                        if (rabbitDist < netInputs[sector])
                        {
                            netInputs[sector] = rabbitDist;
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
            
            for (int i = 0; i < numOfSectors; i++)    // normalize data
            {
                netInputs[i] = sqrAnimalViewRange - netInputs[i];
                netInputs[i] /= sqrAnimalViewRange;
            }
            if(Settings.World.foxHungerInNeuralNet) netInputs[Settings.Fox.neuralNetworkLayers[0]-1] = Health;
            return netInputs;
        }

        protected override float MaxVelocity
        {
            get { return Settings.Fox.foxMaxVelocity; }
        }

        public override float HungerRate { get { return Settings.Fox.foxHungerRate; } }

        static bool firstInit = true;
        private new void Awake()
        {
            numOfSectors = Settings.Fox.neuralNetworkLayers[0];
            if (Settings.World.foxHungerInNeuralNet) numOfSectors--;
            if (firstInit)
            {
                firstInit = false;
                Debug.LogFormat("Fox: sectors: {0}, net size: {1}, values filled by surroundings: {2}, hunger in net: {3}", numOfSectors, Settings.Fox.neuralNetworkLayers[0], numOfSectors, Settings.World.foxHungerInNeuralNet);
            }
            base.Awake();
            closestRabbitInSectors = new Rabbit[numOfSectors];
            netInputs = new float[Settings.Fox.neuralNetworkLayers[0]];
            sqrFoxEatingDistance = Settings.Fox.foxEatingDistance * Settings.Fox.foxEatingDistance;
            sqrAnimalViewRange = Settings.World.animalViewRange * Settings.World.animalViewRange;
        }

        public float Consumed(float amount = 1)
        {
            throw new System.NotImplementedException();
        }

        private void OnDrawGizmosSelected()
        {
            float angle = 360f / numOfSectors;
            for (int i = 0; i < numOfSectors; i++)    
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