using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace World
{
    public abstract class Animal : WorldObject, IAlive, IUpdatable
    {
        public Vector2Int worldSize;
        public bool UseLocalViewSpace;

        protected Vector3 velocity = Vector3.zero;
        protected IAnimalBrain brain;
        protected int deadAtTurn;
        protected int currentTurn = 0;
        public World world;
        public IAnimalBrain Brain { set { brain = value; } }

        protected abstract float MaxVelocity { get; }
        public abstract float HungerRate { get; }
        public float Health { get; set; } = 1f;
        public bool IsAlive => Health > 0f;

        protected int SegmentCount => brain.GetSegmentCount();

        abstract protected void ConsumeFood();

        abstract protected void CollectInfoAboutSurroundings();

        abstract protected float[] CreateNetInputs();

        abstract protected void MultiplyAnimal();

        public virtual Vector3 GetDecision()
        {
            return brain.GetDecision(CreateNetInputs());
        }

        /// <summary>
        /// Parallel update, sets velocity
        /// </summary>
        /// <returns></returns>
        /// 
        void OnDestroy()
        {
            for (var i = gameObject.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(gameObject.transform.GetChild(i).gameObject);
            }
        }
        public bool UpdateTurn()
        {
            //if (!IsAlive)
            //{
            //    throw new Exception("Update on dead animal");
            //}

            world.WorldEvents.Invoke(this, HistoryEventType.POSITION, Position);

            currentTurn++;
            if (currentTurn > deadAtTurn)
            {
                //Debug.Log("Immortal animal " + currentTurn);
                Health = -1;
            }

            // Handle hunger
            Health -= HungerRate * Settings.World.simulationDeltaTime;
            Health = Mathf.Clamp01(Health);
            if (Health <= 0f)
            {
                HandleDeath();
                return false;
            }

            // Get info about surroundings
            CollectInfoAboutSurroundings();

            // Consume food
            ConsumeFood();

            // Get decision from net
            Profiler.BeginSample("run NeuralNet");
            Vector3 decision = GetDecision();
            Profiler.EndSample();

            // Set velocity based on mode
            if (Settings.Player.fastTrainingMode)
            {
            }
                decision = decision / 0.01f;
            if (UseLocalViewSpace)
            {
                Vector3 velChange = Forward * decision.x + Right * decision.z;
                velocity += velChange * MaxVelocity/500;
                if (velocity.magnitude > MaxVelocity)
                    velocity = velocity.normalized * MaxVelocity;
            }
            else
            {
                velocity = decision* MaxVelocity;
            }
            return true;
        }

        protected void HandleDeath()
        {
            world.WorldEvents.Invoke(this, HistoryEventType.DEATH, Position);
        }

        public void UpdatePosition()
        {
            var newPosition = transform.localPosition + (velocity * 0.0002f);
            newPosition.x = Mathf.Clamp(newPosition.x, 0.5f, worldSize.x - 0.5f);
            newPosition.z = Mathf.Clamp(newPosition.z, 0.5f, worldSize.y - 0.5f);
            if (newPosition.y > 0)
                newPosition.y = Mathf.Clamp(newPosition.y - 0.01f, 0, 1000);

            var velDir = velocity.normalized;

            if (velDir.sqrMagnitude > 0)
            {
                transform.forward = velDir;
                //transform.forward *= 0.5f;
                //transform.forward.Normalize();
            }

            transform.localPosition = newPosition;
            position = newPosition;
            forward = transform.forward;
            Right = transform.right;
        }

        protected Vector3 CalculateMultipliedAnimalPosition()
        {
            var radius = Settings.World.multipliedAnimalSpawnRadius;
            var objPosition = Position;
            var xPos = objPosition.x;
            var zPos = objPosition.z;
            return new Vector3(Utils.FloatInRange(xPos - radius, xPos + radius), 0, Utils.FloatInRange(zPos - radius, zPos + radius));
        }

        protected void Awake()
        {
            deadAtTurn = (int)(Settings.World.maxAnimalLifetime / Settings.World.simulationDeltaTime);
            world = transform.parent.GetComponent<World>();
        }

        protected void Start()
        {
            position = gameObject.transform.localPosition;
        }

        protected Vector3 ViewForward => UseLocalViewSpace ? Forward : Vector3.forward;
        protected float ViewAngleOffset => 180f / SegmentCount;
        protected int GetSector(Vector3 offset)
        {
            if (offset.sqrMagnitude == 0f) return 0;
            offset = Quaternion.AngleAxis(-ViewAngleOffset, Vector3.up) * offset.normalized;
            Vector3 cross = Vector3.Cross(ViewForward, offset);
            float angle = Vector3.Angle(ViewForward, offset);
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
            return (int)(angle * SegmentCount / 360f);
        }
    }
}