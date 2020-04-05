using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace World
{
    public abstract class Animal : WorldObject, IAlive, IUpdatable
    {
        public Vector2Int worldSize;
        public LayerMask feedOnLayer;
        private Collider[] neighbourColliders;
        public List<INeuralNetInputProvider> prayList;
        public List<INeuralNetInputProvider> predatorList;

        protected Vector3 velocity = Vector3.zero;
        protected IBigBrain brain;
        public IBigBrain Brain { set { brain = value; } }

        protected abstract float MaxVelocity { get; }
        public abstract float HungerRate { get; }
        public float Health { get; set; } = 1f;
        public bool IsAlive => Health > 0f;

        abstract protected void ConsumeFood();
 
        public bool UpdateTurn() 
        {
            //Physics.OverlapSphereNonAlloc(transform.position, Settings.World.animalViewRange, neighbourColliders);

            if (!IsAlive) return false;

            Health -= HungerRate * Settings.World.simulationDeltaTime;
            if (Health <= 0f)
            {
                velocity = Vector3.zero;
                return false;
            }

            ConsumeFood();

            float[] inputs = null;
            Vector3 decision = brain.GetDecision(inputs);


            if (Settings.Player.fastTrainingMode)
            {
                velocity = decision * Settings.World.simulationDeltaTime / World.deltaTime;
            }
            else
            {
                velocity = decision;
            }

            //Profiler.BeginSample("overlap");
            //var colliders = Physics.OverlapSphere(transform.position, Settings.World.animalViewRange);
            //Profiler.EndSample();
            //int num = 0;
            //foreach (var collider in colliders)
            //{
            //    if (collider.gameObject.transform == transform || collider.gameObject.transform.parent != transform.parent) continue;

            //    num++;
            //}
            prayList = null;
            predatorList = null;
            return true;
        }

        public override float GetInputValue()
        {
            throw new System.NotImplementedException();
        }

        private void Update()
        {
            var newPosition = transform.localPosition + (MaxVelocity * velocity * Time.deltaTime);
            newPosition.x = Mathf.Clamp(newPosition.x, 0.5f, worldSize.x - 0.5f);
            newPosition.z = Mathf.Clamp(newPosition.z, 0.5f, worldSize.y - 0.5f);

            velocity = (newPosition - transform.localPosition) / Time.deltaTime;
            if (velocity.sqrMagnitude > 0) transform.forward = velocity.normalized;

            transform.localPosition = newPosition;
            position = newPosition;
        }

        private new void Start()
        {
            base.Start();
            neighbourColliders = new Collider[(int)(Settings.World.animalViewRange * Settings.World.animalViewRange)];
        }
    }
}