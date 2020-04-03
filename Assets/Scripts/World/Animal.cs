using System;
using UnityEngine;

namespace World
{
    public abstract class Animal : MonoBehaviour, IAlive, IUpdatable
    {
        public Vector2Int worldSize;
        public LayerMask feedOnLayer;

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
            if (!IsAlive) return false;

            Health -= HungerRate * Time.fixedDeltaTime;
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
                velocity = decision * Time.fixedDeltaTime / Time.deltaTime;
            }
            else
            {
                velocity = decision;
            }

            var colliders = Physics.OverlapSphere(transform.position, Settings.World.animalViewRange);
            int num = 0;
            foreach (var collider in colliders)
            {
                if (collider.gameObject.transform == transform || collider.gameObject.transform.parent != transform.parent) continue;

                num++;
            }
            return true;
        }

        private void Update()
        {
            var newPosition = transform.localPosition + (MaxVelocity * velocity * Time.deltaTime);
            newPosition.x = Mathf.Clamp(newPosition.x, 0.5f, worldSize.x - 0.5f);
            newPosition.z = Mathf.Clamp(newPosition.z, 0.5f, worldSize.y - 0.5f);

            velocity = (newPosition - transform.localPosition) / Time.deltaTime;
            if (velocity.sqrMagnitude > 0) transform.forward = velocity.normalized;

            transform.localPosition = newPosition;
        }

 
    }
}