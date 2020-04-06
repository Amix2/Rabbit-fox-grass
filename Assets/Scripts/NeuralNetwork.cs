using System;
using System.Collections.Generic;
using NumSharp;
using UnityEngine;

public class NeuralNetwork : IBigBrain
{
    public NDArray[] Weights => _weights;
    public NDArray[] Biases => _biases;
    public int[] Layers => _layers;

    private readonly int[] _layers;
    private NDArray[] _weights;
    private NDArray[] _biases;


    public NeuralNetwork(int[] layers)
    {
        CheckLayers(layers);

        _layers = layers;
        GenerateRandomWeights();
    }

    public NeuralNetwork(int[] layers, NDArray[] weights, NDArray[] biases)
    {
        CheckLayers(layers);
        CheckWeights(layers, weights, biases);

        _layers = layers;
        _weights = weights;
        _biases = biases;
    }


    public Vector3 GetDecision(NDArray input)
    {
        // if input is not in range (0,1) we have to do normalization
        // NormalizeVector(input);
        var prevNeurons = input;

        for (var i = 0; i < _layers.Length - 1; i++)
        {
            var res = np.matmul(_weights[i], prevNeurons).reshape(-1) + _biases[i];
            NormalizeVector(res);
            prevNeurons = res;
        }

        var decision = new Vector3(Convert.ToSingle((double) prevNeurons[0]), 0.0f,
            Convert.ToSingle((double) prevNeurons[1]));
        decision.x -= 0.5f;
        decision.z -= 0.5f;
        decision *= 2f;
        if (decision.sqrMagnitude > 1)
        {
            return decision.normalized;
        }
        return decision;
    }

    private void GenerateRandomWeights()
    {
        var weights = new List<NDArray>();
        var biases = new List<NDArray>();

        for (var i = 1; i < _layers.Length; i++)
        {
            var layerWeights = np.random.randn(_layers[i - 1] * _layers[i]).reshape(_layers[i], -1);
            weights.Add(layerWeights);

            var layerBiases = np.random.randn(_layers[i]);
            biases.Add(layerBiases);
        }

        _weights = weights.ToArray();
        _biases = biases.ToArray();
    }

    private static void CheckLayers(int[] layers)
    {
        if (layers.Length <= 1) throw new Exception($"Cannot create network for {layers.Length} layer");

        foreach (var layer in layers)
            if (layer <= 0)
                throw new Exception($"Layer cannot have {layer} neurons");
    }

    private static void CheckWeights(int[] layers, NDArray[] weights, NDArray[] biases)
    {
        if (biases.Length != layers.Length - 1) throw new Exception("Wrong number of layers weights");
        if (weights.Length != layers.Length - 1) throw new Exception("Wrong number of layers biases");

        for (var i = 0; i < layers.Length - 1; i++)
        {
            if (weights[i].shape[0] != layers[i + 1]) throw new Exception("Wrong number of neurons in layer weights");
            if (weights[i].shape[1] != layers[i + 1])
                throw new Exception("Wrong number of neurons from previous layer");
            if (biases[i].shape[0] != layers[i + 1]) throw new Exception("Wrong number of neurons in layer biases");
        }
    }

    private static string ArrayToString<T>(T[] array)
    {
        string ret = "";
        foreach (var element in array)
        {
            ret += element + " ";
        }

        return ret;
    }

    private static void NormalizeVector(NDArray input)
    {
        for (var i = 0; i < input.shape[0]; i++)
        {
            input[i] = Sigmoid(input[i]);
        }
    }

    private static double Sigmoid(double value)
    {
        var k = Math.Exp(value);
        return k / (1.0f + k);
    }
}