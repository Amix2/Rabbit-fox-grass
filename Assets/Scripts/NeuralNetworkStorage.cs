using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace DefaultNamespace
{
    public class NeuralNetworkStorage
    {
        public static void SaveToFile(string filePath, NeuralNetwork[] networks, int[] fitnesses)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("{ \"networks\":");
            stringBuilder.AppendLine("[");
            for (int i = 0; i < networks.Length; i++)
            {
                stringBuilder.Append(networks[i].ToString(fitnesses[i]));
                if (i < networks.Length - 1) stringBuilder.AppendLine(",");
            }

            stringBuilder.AppendLine("] }");

            System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
        }

        public static NeuralNetwork[] ReadFromFile(string filePath)
        {
            var fileContent = System.IO.File.ReadAllText(filePath);
            var networksObject = JObject.Parse(fileContent);
            var networks = new List<NeuralNetwork>();

            foreach (var networkObject in networksObject["networks"].Children())
            {
                networks.Add(NeuralNetwork.NetworkFromString(networkObject));
            }

            return networks.ToArray();
        }
    }
}