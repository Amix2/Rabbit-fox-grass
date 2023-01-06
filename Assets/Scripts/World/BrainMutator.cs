using DefaultNamespace;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainMutator
{
    public static IAnimalBrain Mutate(IAnimalBrain animalBrain)
    {
        return new DecisionTree(animalBrain as DecisionTree);
    }

    public static IAnimalBrain Mutate(NeuralNetwork animalBrain)
    {
        return NeuralNetworkMutator.Mutate(animalBrain);
    }

    public static IAnimalBrain Mutate(DecisionTree animalBrain)
    {
        return new DecisionTree(animalBrain);
    }
}
