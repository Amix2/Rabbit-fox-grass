using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class RandomEnum
{
    static System.Random _R = new System.Random();
    public static T Value<T>()
    {
        var v = Enum.GetValues(typeof(T));
        return (T)v.GetValue(_R.Next(v.Length));
    }
}
public abstract class ITreeNode
{
    public static int MaxValue = 1000;
    public abstract Vector3 GetDecision(float[] floats);
    public abstract string ToJsonString();

    public abstract void AsBestToFile(StringBuilder stringBuilder, string parent, string add);
    public abstract bool IsLeaf();

}

public class TreeLeaf : ITreeNode
{
    public enum Type
    { N, E, S, W };

    [SerializeField]
    public Type type;
    public string CLASS_NAME = "TreeLeaf";

    public TreeLeaf(Type type)
    {
        this.type = type;
    }
    public TreeLeaf() 
    {
        type = RandomEnum.Value<Type>();   
    }
    public TreeLeaf(string jsonString)
    {
        TreeLeaf obj = JsonConvert.DeserializeObject<TreeLeaf>(jsonString);
        type = obj.type;
    }

    public override string ToJsonString()
    { return JsonConvert.SerializeObject(this, Formatting.None); }



    public override Vector3 GetDecision(float[] floats)
    {
        switch (type)
        {
            case Type.N:
                return new Vector3(1, 0, 0);

            case Type.E:
                return new Vector3(0, 0, 1);

            case Type.S:
                return new Vector3(-1, 0, 0);

            case Type.W:
                return new Vector3(0, 0, -1);
        }
        return new Vector3(0, 0, 0);
    }

    public override void AsBestToFile(StringBuilder stringBuilder, string parent, string add)
    {
        stringBuilder.Append(String.Format("{{\"parent\":\"{0}\", \"id\":\"{1}\", \"value\":\"{2}\"}},", parent, parent+add,type.ToString()));
    }

    public override bool IsLeaf()
    {
        return true;
    }
}

public class TreeNode : ITreeNode
{
    public ITreeNode lessChild, moreChild;
    public enum Operator { MIN, MAX, AVG, ADD };
    Operator leftOper;
    Operator rightOper;
    List<int> leftValues;
    List<int> rightValues;
    public string CLASS_NAME = "TreeNode";

    public TreeNode(ITreeNode lessChild, ITreeNode moreChild, Operator leftOper, List<int> leftValues, Operator rightOper, List<int> rightValues)
    {
        this.lessChild = lessChild;
        this.moreChild = moreChild;
        this.leftOper = leftOper;
        this.rightOper = rightOper;
        this.leftValues = leftValues;
        this.rightValues = rightValues;
    }

    public TreeNode(int valuesSize, float valueProb)
    {
        this.lessChild = null;
        this.moreChild = null;
        this.leftOper = RandomEnum.Value<Operator>();
        this.rightOper = RandomEnum.Value<Operator>();

        this.leftValues = RandomValues(valueProb, valuesSize);
        this.rightValues = RandomValues(valueProb, valuesSize);
    }

    List<int> RandomValues(float prob, int valuesSize)
    {
        HashSet<int> set = new HashSet<int>();
        bool numValuePresent = false;

        while(UnityEngine.Random.Range(0.0f, 1.0f) < prob / Mathf.Max(1, set.Count) || set.Count == 0)
        {
            int value = UnityEngine.Random.Range(0, valuesSize + 1);
            if(value == valuesSize && !numValuePresent)
            {
                numValuePresent = true;
                set.Add(-1*UnityEngine.Random.Range(0, MaxValue));
            }
            else
            {
                set.Add(value);
            }
        }

        return set.ToList();
    }

    public void VisitAll(Action<ITreeNode> visitor)
    {
        visitor(this);

        if(lessChild != null)
        {
            if (lessChild.IsLeaf())
                visitor(lessChild);
            else
                (lessChild as TreeNode).VisitAll(visitor);
        }
        if (moreChild != null)
            if (moreChild.IsLeaf())
                visitor(moreChild);
            else
                (moreChild as TreeNode).VisitAll(visitor);
    }

    public TreeNode(string jsonString)
    {
        Dictionary<string, string> objDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
        leftOper = (Operator)Enum.Parse(typeof(Operator), objDict["leftOper"]);
        rightOper = (Operator)Enum.Parse(typeof(Operator), objDict["rightOper"]);
        leftValues = JsonConvert.DeserializeObject<List<int>>(objDict["leftValues"]);
        rightValues = JsonConvert.DeserializeObject<List<int>>(objDict["rightValues"]);
        string lessChildClassName = JsonConvert.DeserializeObject<Dictionary<string, string>>(objDict["LessChild"])["CLASS_NAME"];
        if(lessChildClassName == "TreeNode")
        {
            lessChild = new TreeNode(objDict["LessChild"]);
        }
        else
        {
            lessChild = new TreeLeaf(objDict["LessChild"]);
        }
        string moreChildClassName = JsonConvert.DeserializeObject<Dictionary<string, string>>(objDict["MoreChild"])["CLASS_NAME"];
        if (moreChildClassName == "TreeNode")
        {
            moreChild = new TreeNode(objDict["MoreChild"]);
        }
        else
        {
            moreChild = new TreeLeaf(objDict["MoreChild"]);
        }
    }

    public override string ToJsonString()
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        dict.Add("CLASS_NAME", CLASS_NAME);
        dict.Add("leftOper", leftOper.ToString());
        dict.Add("rightOper", rightOper.ToString());
        dict.Add("leftValues", JsonConvert.SerializeObject(leftValues, Formatting.None));
        dict.Add("rightValues", JsonConvert.SerializeObject(rightValues, Formatting.None));
        dict.Add("LessChild", lessChild.ToJsonString());
        dict.Add("MoreChild", moreChild.ToJsonString());

        return JsonConvert.SerializeObject(dict, Formatting.None); 
    
    }

    public override Vector3 GetDecision(float[] floats)
    {
        float leftVal = GetValue(floats, leftValues);
        float rightVal = GetValue(floats, rightValues);
        return leftVal < rightVal ? lessChild.GetDecision(floats) : moreChild.GetDecision(floats);

        float GetValue(float[] floats, List<int> values)
        {
            float leftVal;
            switch (leftOper)
            {
                case Operator.MIN:
                    leftVal = MIN(floats, values);
                    break;
                case Operator.MAX:
                    leftVal = MAX(floats, values);
                    break;
                case Operator.AVG:
                    leftVal = AVG(floats, values);
                    break;
                case Operator.ADD:
                    leftVal = ADD(floats, values);
                    break;
                default:
                    leftVal = 0;
                    break;
            }

            return leftVal;
        }
    }

    private float ADD(float[] floats, List<int> values)
    {
        float sum = 0;
        foreach(int val in values)
            if (val >= 0)
                sum += floats[val];
            else
                sum += ((float)Math.Abs(val)) / MaxValue;
        return sum;
    }
    private float MIN(float[] floats, List<int> values)
    {
        float minval = 0;
        foreach (int val in values)
            if (val >= 0)
                minval = Math.Min(minval, floats[val]);
            else
                minval = Math.Min(minval, ((float)Math.Abs(val))/MaxValue);
        return minval;
    }
    private float MAX(float[] floats, List<int> values)
    {
        float minval = 0;
        foreach (int val in values)
            if (val >= 0)
                minval = Math.Max(minval, floats[val]);
            else
                minval = Math.Max(minval, ((float)Math.Abs(val)) / MaxValue);   
        return minval;
    }
    private float AVG(float[] floats, List<int> values)
    {
        return ADD(floats, values) / values.Count;
    }

    public override void AsBestToFile(StringBuilder stringBuilder, string parent, string add)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        dict.Add("CLASS_NAME", CLASS_NAME);
        dict.Add("leftOper", leftOper.ToString());
        dict.Add("rightOper", rightOper.ToString());
        dict.Add("leftValues", JsonConvert.SerializeObject(leftValues, Formatting.None));
        dict.Add("rightValues", JsonConvert.SerializeObject(rightValues, Formatting.None));
        dict.Add("LessChild", lessChild.ToJsonString());
        dict.Add("MoreChild", moreChild.ToJsonString());

        stringBuilder.Append(String.Format("{{\"parent\":\"{0}\", \"id\":\"{1}\", \"value\":\"{2}({3}) < {4}({5})\"}},", parent, parent+add
            , dict["leftOper"], dict["leftValues"], dict["rightOper"], dict["rightValues"]));
        lessChild.AsBestToFile(stringBuilder, parent + add, "L");
        moreChild.AsBestToFile(stringBuilder, parent + add, "R");

    }

    public override bool IsLeaf()
    {
        return false;
    }
}

public class DecisionTree : IAnimalBrain
{
    int segmentCount;
    int maxSize;
    TreeNode root;
    public DecisionTree(string jsonString)
    {
        Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
        root =  new TreeNode(dict["TreeData"]);
        segmentCount = int.Parse(dict["SegmentCount"]);
        maxSize = int.Parse(dict["MaxSize"]);
    }
    public DecisionTree(int[] treeParams)
    {
        segmentCount = UnityEngine.Random.Range(3, treeParams[0]);
        maxSize = treeParams[1];
        root = CreateRandom(0.5f, 1);
    }

    public DecisionTree(DecisionTree other)
    {
        float timeStart = Time.realtimeSinceStartup;
        segmentCount = other.segmentCount;
        maxSize = other.maxSize;

        root = new TreeNode(other.root.ToJsonString());
        //root = CreateRandom(0.5f, 1);
        //Debug.Log("DecisionTree Mutate time: " + (Time.realtimeSinceStartup - timeStart));

    }

    public override Vector3 GetDecision(float[] floats)
    {
        return root.GetDecision(floats);
    }

    public override int GetSegmentCount()
    {
        return segmentCount;
    }

    TreeNode CreateRandom(float childProp, float valueProp)
    {
        float timeStart = Time.realtimeSinceStartup;
        TreeNode root = new TreeNode(GetSegmentCount(), valueProp);
        int nodeCount = 0;

        Action<ITreeNode> addNodesVisitor = delegate (ITreeNode node)
        {
            Debug.Assert(!node.IsLeaf());
            TreeNode treeNode = node as TreeNode;
            if (UnityEngine.Random.Range(0.0f, 1.0f) < childProp && nodeCount < maxSize)
            {
                nodeCount++;
                treeNode.lessChild = new TreeNode(GetSegmentCount(), valueProp);
            }
            if (UnityEngine.Random.Range(0.0f, 1.0f) < childProp && nodeCount < maxSize)
            {
                nodeCount++;
                treeNode.moreChild = new TreeNode(GetSegmentCount(), valueProp);
            }
        };
        root.VisitAll(addNodesVisitor);

        AddLeafsToEndings(root);

        //Debug.Log("DecisionTree CreateRandom time: " + (Time.realtimeSinceStartup - timeStart) + ", size: " + nodeCount);
        return root;
    }

    private static void AddLeafsToEndings(TreeNode root)
    {
        Action<ITreeNode> addLeafsVisitor = delegate (ITreeNode node)
        {
            if (!node.IsLeaf())
            {
                TreeNode treeNode = node as TreeNode;
                if (treeNode.lessChild == null)
                    treeNode.lessChild = new TreeLeaf();
                if (treeNode.moreChild == null)
                    treeNode.moreChild = new TreeLeaf();
            }
            else
            {
                if (node == null)
                {
                    int t = 0;
                }

            }
        };

        root.VisitAll(addLeafsVisitor);
    }

    public override void AddToFile(float fitness, StringBuilder stringBuilder)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        dict["Fitness"] = fitness.ToString();
        dict["TreeData"] = root.ToJsonString();
        dict["SegmentCount"] = segmentCount.ToString();
        dict["MaxSize"] = maxSize.ToString();
        stringBuilder.AppendLine(JsonConvert.SerializeObject(dict, Formatting.None));
    }
    public override void AsBestToFile(StringBuilder stringBuilder)
    {
        stringBuilder.Append(String.Format("{{\"id\":\"{0}\", \"value\":\"segmentCount: {1}\"}},", "T", segmentCount));
        root.AsBestToFile(stringBuilder, "T", "R");
        stringBuilder.Remove(stringBuilder.Length - 1, 1);
    }
}