using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using MathNet.Numerics.Random;

namespace DefaultNamespace
{
    public class NeuralNetworkMutator
    {
        public static NeuralNetwork Mutate(NeuralNetwork network)
        {
            var copiedMatrices = CopyMatrices(network);
            Matrix<float>[] layersWeights = copiedMatrices.Item1;
            Matrix<float>[] layersBiases = copiedMatrices.Item2;

            switch(Settings.NeuralMutationSettings.mutationStrategyEnum)
            {
                case MutationStrategyEnum.Random:
                    return WithRandomStrategy(network.Layers, layersWeights, layersBiases);
            }

            throw new Exception("Selected mutation strategy is incorrect or not available yet");
        }

        private static NeuralNetwork WithRandomStrategy(int[] layers, Matrix<float>[]layersWeights,Matrix<float>[] layersBiases)
        {
            var random = new MersenneTwister();
            double weightsRange = Settings.Player.neuralNetworkWeightsRange[1] - Settings.Player.neuralNetworkWeightsRange[0];
            double biasRange = Settings.Player.neuralNetworkBiasRange[1] - Settings.Player.neuralNetworkBiasRange[0];

            for (var i = 0; i < layers.Length - 1; i++)
            {
                for (var j = 0; j < layersWeights[i].RowCount;j++)
                {
                    for (var k = 0; k < layersWeights[i].ColumnCount;k++)
                    {
                        if (random.NextDouble() < Settings.NeuralMutationSettings.mutationProbability)
                        {
                            layersWeights[i][j, k] = Convert.ToSingle(random.NextDouble() * weightsRange + Settings.Player.neuralNetworkWeightsRange[0]);
                        }
                    }
                }
                for (var j = 0; j < layersBiases[i].RowCount;j++)
                {
                    if (random.NextDouble() < Settings.NeuralMutationSettings.mutationProbability)
                    {
                        layersBiases[i][j, 0] = Convert.ToSingle(random.NextDouble() * biasRange + Settings.Player.neuralNetworkBiasRange[0]);
                    }
                }
            }
            
            return new NeuralNetwork(layers, layersWeights, layersBiases);
        }

        private static Tuple<Matrix<float>[], Matrix<float>[]> CopyMatrices(NeuralNetwork network)
        {
            var layersWeights = new List<Matrix<float>>();
            var layersBiases = new List<Matrix<float>>();

            for (var i = 0; i < network.Weights.Length; i++)
            {
                layersWeights.Add(CreateCopyOfMatrix(network.Weights[i]));
            }
            
            for (var i = 0; i < network.Biases.Length; i++)
            {
                layersBiases.Add(CreateCopyOfMatrix(network.Biases[i]));
            }
            
            return Tuple.Create(layersWeights.ToArray(),layersBiases.ToArray());
        }

        private static Matrix<float> CreateCopyOfMatrix(Matrix<float> toCopy)
        {
            Matrix<float> copied = DenseMatrix.Create(toCopy.RowCount, toCopy.ColumnCount, 0.0f);
            for (var i = 0; i < copied.RowCount; i++)
            {
                for (var j = 0; j < copied.ColumnCount; j++)
                {
                    copied[i, j] = toCopy[i, j];
                }
            }

            return copied;
        }
    }
}