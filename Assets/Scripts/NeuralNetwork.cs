using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using MathNet.Numerics.Random;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class NeuralNetwork
{
    public Matrix<float>[] Weights => _weights;
    public Matrix<float>[] Biases => _biases;
    public int[] Layers => _layers;

    private readonly int[] _layers;
    private Matrix<float>[] _weights;
    private Matrix<float>[] _biases;


    public NeuralNetwork(int[] layers)
    {
        CheckLayers(layers);

        _layers = layers;
        GenerateRandomWeights();
    }

    public NeuralNetwork(int[] layers, Matrix<float>[] weights, Matrix<float>[] biases)
    {
        CheckLayers(layers);
        CheckWeights(layers, weights, biases);

        _layers = layers;
        _weights = weights;
        _biases = biases;
    }


    public Vector3 GetDecision(float[] input)
    {
        for(int i=1; i<input.Length; i++)
        {
            //if(input[i]< 1) Debug.Log(input[i]);
        }
        var prevNeurons = PreprocessInput(input);
        // if input is not in range (0,1) we have to do normalization
        // NormalizeVector(input);

        for (var i = 0; i < _layers.Length - 1; i++)
        {
            var res = _weights[i] * prevNeurons + _biases[i];
            NormalizeVector(res);
           // Debug.Log(res);

            prevNeurons = res;
        }

        var decision = new Vector3();
        decision.x = 2 * Convert.ToSingle((double) prevNeurons[0, 0]) - 1f;
        decision.y = 0f;
        decision.z = 2 * Convert.ToSingle((double) prevNeurons[1, 0]) - 1f;

        return decision.sqrMagnitude > 1 ? decision.normalized : decision;
    }

    public static NeuralNetwork NetworkFromString(JToken network)
    {
        var layersWeights = new List<Matrix<float>>();
        var layersBiases = new List<Matrix<float>>();

        foreach (var matrixToken in network["weights"].Children().ToList())
        {
            layersWeights.Add(StringTo2DMatrix(matrixToken));
        }

        foreach (var matrixToken in network["biases"].Children().ToList())
        {
            layersBiases.Add(StringTo1DMatrix(matrixToken));
        }

        var layers = LayersFromToken(network["weights"]);
        return new NeuralNetwork(layers, layersWeights.ToArray(), layersBiases.ToArray());
    }

    public string ToString(int fitness)
    {
        var stringBuilder = new StringBuilder().AppendLine("{");

        stringBuilder.AppendLine("\"fitness\":" + fitness.ToString() + ",");

        stringBuilder.Append("\"weights\":" + MatrixArrayToString(_weights));

        stringBuilder.AppendLine(",");
        stringBuilder.Append("\"biases\":" + MatrixArrayToString(_biases));

        stringBuilder.Append("}");

        return stringBuilder.ToString();
    }

    private static void CheckLayers(int[] layers)
    {
        if (layers.Length <= 1) throw new Exception($"Cannot create network for {layers.Length} layer");

        foreach (var layer in layers)
            if (layer <= 0)
                throw new Exception($"Layer cannot have {layer} neurons");
    }

    private static void CheckWeights(int[] layers, Matrix<float>[] weights, Matrix<float>[] biases)
    {
        if (biases.Length != layers.Length - 1) throw new Exception("Wrong number of layers weights");
        if (weights.Length != layers.Length - 1) throw new Exception("Wrong number of layers biases");

        for (var i = 0; i < layers.Length - 1; i++)
        {
            if (weights[i].RowCount != layers[i + 1]) throw new Exception("Wrong number of neurons in layer weights");
            if (weights[i].ColumnCount != layers[i])
                throw new Exception("Wrong number of neurons from previous layer, layer size:" + layers[i + 1] +
                                    " actual num of columns in weights: " + weights[i].ColumnCount);
            if (biases[i].RowCount != layers[i + 1]) throw new Exception("Wrong number of neurons in layer biases");
        }
    }

    private static void NormalizeVector(Matrix<float> input)
    {
        for (var i = 0; i < input.RowCount; i++)
        {
            input[i, 0] = Sigmoid(input[i, 0]);
        }
    }

    private static float Sigmoid(float value)
    {
        if (value >= 4f) return 1f;
        float tmp = 1f - 0.25f * value;
        tmp *= tmp;
        tmp *= tmp;
        tmp *= tmp;
        tmp *= tmp;
        return 1f / (1f + tmp);
    }

    private Matrix<float> PreprocessInput(float[] input)
    {
        Matrix<float> preprocessed = DenseMatrix.Create(input.Length, 1, 0.0f);
        for (var i = 0; i < input.Length; i++)
        {
            preprocessed[i, 0] = input[i];
        }

        return preprocessed;
    }

    private void GenerateRandomWeights()
    {
        var weights = new List<Matrix<float>>();
        var biases = new List<Matrix<float>>();

        for (var i = 1; i < _layers.Length; i++)
        {
            Matrix<float> layerWeights = DenseMatrix.Create(_layers[i], _layers[i - 1], 0.0f);
            Matrix<float> layerBiases = DenseMatrix.Create(_layers[i], 1, 0.0f);
            var random = new MersenneTwister();
            
            double weightsRange = Settings.Player.neuralNetworkWeightsRange[1] - Settings.Player.neuralNetworkWeightsRange[0];
            double biasRange = Settings.Player.neuralNetworkBiasRange[1] - Settings.Player.neuralNetworkBiasRange[0];
            for (var j = 0; j < layerWeights.RowCount; j++)
            {
                for (var k = 0; k < layerWeights.ColumnCount; k++)
                {
                    layerWeights[j, k] = Convert.ToSingle(random.NextDouble() * weightsRange + Settings.Player.neuralNetworkWeightsRange[0]);
                }
            }

            for (var j = 0; j < layerBiases.RowCount; j++)
            {
                layerBiases[j, 0] = Convert.ToSingle(random.NextDouble() * biasRange + Settings.Player.neuralNetworkBiasRange[0]);
            }

            weights.Add(layerWeights);
            biases.Add(layerBiases);
        }

        _weights = weights.ToArray();
        _biases = biases.ToArray();
    }

    private static string MatrixArrayToString(Matrix<float>[] array)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("[");

        for (var i = 0; i < array.Length; i++)
        {
            stringBuilder.Append(MatrixToString(array[i]));
            if (i < array.Length - 1) stringBuilder.AppendLine(",");
        }

        stringBuilder.AppendLine();
        stringBuilder.Append("]");

        return stringBuilder.ToString();
    }

    private static string MatrixToString(Matrix<float> matrix)
    {
        var stringBuilder = new StringBuilder();
        var hasMoreThanOneCol = matrix.ColumnCount > 1;

        stringBuilder.AppendLine("[");
        for (var i = 0; i < matrix.RowCount; i++)
        {
            stringBuilder.Append(RowToString(matrix, i, hasMoreThanOneCol));
            if (i < matrix.RowCount - 1 && hasMoreThanOneCol) stringBuilder.AppendLine(",");
            if (i < matrix.RowCount - 1 && !hasMoreThanOneCol) stringBuilder.Append(",");
        }

        stringBuilder.AppendLine();
        stringBuilder.Append("]");

        return stringBuilder.ToString();
    }

    private static string RowToString(Matrix<float> matrix, int rowIndex, bool hasMoreThanOneCol)
    {
        var stringBuilder = new StringBuilder();

        if (hasMoreThanOneCol) stringBuilder.Append("[");
        for (var j = 0; j < matrix.ColumnCount; j++)
        {
            stringBuilder.Append(matrix[rowIndex, j].ToString(CultureInfo.InvariantCulture));
            if (j < matrix.ColumnCount - 1) stringBuilder.Append(",");
        }

        if (hasMoreThanOneCol) stringBuilder.Append("]");

        return stringBuilder.ToString();
    }

    private static Matrix<float> StringTo2DMatrix(JToken token)
    {
        var rowsNum = token.Children().Count();
        var colNum = token[0].Children().Count();
        var matrix = DenseMatrix.Create(rowsNum, colNum, 0f);

        for (var i = 0; i < rowsNum; i++)
        {
            for (var j = 0; j < colNum; j++)
            {
                matrix[i, j] = token[i].Children().ToArray()[j].Value<float>();
            }
        }

        return matrix;
    }

    private static Matrix<float> StringTo1DMatrix(JToken token)
    {
        var rowsNum = token.Children().Count();
        var matrix = DenseMatrix.Create(rowsNum, 1, 0f);

        for (var i = 0; i < rowsNum; i++)
        {
            matrix[i, 0] = token[i].Value<float>();
        }

        return matrix;
    }

    private static int[] LayersFromToken(JToken token)
    {
        var layers = new List<int>();

        foreach (var child in token.Children())
        {
            layers.Add(child[0].Children().Count());
        }

        layers.Add(token[token.Count() - 1].Children().Count());
        return layers.ToArray();
    }
}