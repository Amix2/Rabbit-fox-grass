using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class WorldHistory
    {
        public List<Vector3> rabbitsDeath;
        public List<Vector3> rabbitsBirth;
        public List<Vector3> foxesDeath;
        public List<Vector3> foxesBirth;
        public List<Vector3> grassPositions;
        public int lifeTime = 0;

        public WorldHistory()
        {
            rabbitsDeath = new List<Vector3>();
            rabbitsBirth = new List<Vector3>();
            foxesDeath = new List<Vector3>();
            foxesBirth = new List<Vector3>();
            grassPositions = new List<Vector3>();
        }

        public void RabbitDeath(Vector3 pos)
        {
            rabbitsDeath.Add(pos);
        }

        public void RabbitBirth(Vector3 pos)
        {
            rabbitsBirth.Add(pos);
        }

        public void FoxDeath(Vector3 pos)
        {
            foxesDeath.Add(pos);
        }

        public void FoxBirth(Vector3 pos)
        {
            foxesBirth.Add(pos);
        }

        public void Grass(Vector3 pos)
        {
            grassPositions.Add(pos);
        }
    }
}