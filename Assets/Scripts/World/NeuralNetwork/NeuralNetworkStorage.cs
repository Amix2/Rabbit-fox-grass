using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace DefaultNamespace
{
    public class NeuralNetworkStorage
    {
        public static void SaveToFile(string filePath, NeuralNetwork[] networks, float[] fitnesses)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("{ \"brains\":");
            stringBuilder.AppendLine("[");
            for (int i = 0; i < networks.Length; i++)
            {
                SaveToFile(networks[i], fitnesses[i], stringBuilder);
                if (i < networks.Length - 1) stringBuilder.AppendLine(",");
            }

            stringBuilder.AppendLine("] }");

            System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
        }

        private static void SaveToFile(NeuralNetwork network, float fitness, StringBuilder stringBuilder)
        {
            stringBuilder.Append(network.ToString(fitness));
        }

        internal static void SaveToFile(string filePath, IAnimalBrain[] animalBrains, float[] fitnesses)
        {
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("{ \"brains\":");
                stringBuilder.AppendLine("[");
                System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
            }
            for (int i = 0; i < animalBrains.Length; i++)
            {
                var stringBuilder = new StringBuilder();
                animalBrains[i].AddToFile(fitnesses[i], stringBuilder);
                if (i < animalBrains.Length - 1) stringBuilder.AppendLine(",");
                System.IO.File.AppendAllText(filePath, stringBuilder.ToString());
            }
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("],");
                stringBuilder.AppendLine("\"best\":");
                stringBuilder.AppendLine("[");
                for (int i = 0; i < Math.Min(animalBrains.Length, 3); i++)
                {
                    stringBuilder.AppendLine("[");
                    animalBrains[i].AsBestToFile(stringBuilder);
                    stringBuilder.AppendLine("]");
                    if (i < Math.Min(animalBrains.Length-1, 3-1)) stringBuilder.AppendLine(",");
                }
                stringBuilder.AppendLine("] }");
                System.IO.File.AppendAllText(filePath, stringBuilder.ToString());
            }
        }

        public static IAnimalBrain[] ReadFromFile(string filePath)
        {
            var fileContent = System.IO.File.ReadAllText(filePath);
            var networksObject = JObject.Parse(fileContent);
            var networks = new List<IAnimalBrain>();

            foreach (var networkObject in networksObject["brains"].Children())
            {
                networks.Add(new DecisionTree(networkObject.ToString()));
            }

            return networks.ToArray();
        }
    }
}