using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using World;

public class FitnessGraph : MonoBehaviour
{
    public bool showPlot = true;
    public WorldCreator worldCreator;
    public float plotRefreshTime = 1;

    private string PlotDataDirPath => Application.streamingAssetsPath + "/PlotsData/";
    private string PlotScriptDirPath => Application.streamingAssetsPath + "/Plots/";
    private string rabbitDataFilePath;
    private string FullRabbitDataFilePath => PlotDataDirPath + rabbitDataFilePath;
    private string foxDataFilePath;
    private string FullFoxDataFilePath => PlotDataDirPath + foxDataFilePath;

    private GraphScript rabbitGraphScript;
    private GraphScript foxGraphScript;

    private readonly string pythonScriptName = "runPlot.bat";

    // Start is called before the first frame update
    private void Start()
    {
        if (!Directory.Exists(PlotDataDirPath))
        {
            Directory.CreateDirectory(PlotDataDirPath);
        }

        UnityEngine.Debug.Log(Application.streamingAssetsPath);
        if (!showPlot || !Settings.World.collectHistory)
        {
            Destroy(this.gameObject);
            return;
        }

        try
        {
            string sessionFileName = DateTime.Now.ToShortDateString().Replace("/", "-") + "-" +
                              DateTime.Now.ToLongTimeString().Replace(":", "-") +
                              ".txt";

            rabbitDataFilePath = "rabbit_" + sessionFileName;
            foxDataFilePath = "fox_" + sessionFileName;

            File.Create(FullRabbitDataFilePath).Close();
            using (StreamWriter writer = File.AppendText(FullRabbitDataFilePath))
            {
                writer.WriteLine("Rabbit fitness");
                writer.WriteLine("Max;Avg;Min");
                writer.Close();
            }
            rabbitGraphScript = new GraphScript(PlotScriptDirPath + pythonScriptName, FullRabbitDataFilePath, plotRefreshTime);
            rabbitGraphScript.Run();

            File.Create(FullFoxDataFilePath).Close();
            using (StreamWriter writer = File.AppendText(FullFoxDataFilePath))
            {
                writer.WriteLine("Fox fitness");
                writer.WriteLine("Max;Avg;Min");
                writer.Close();
            }
            foxGraphScript = new GraphScript(PlotScriptDirPath + pythonScriptName, FullFoxDataFilePath, plotRefreshTime);
            foxGraphScript.Run();

            //StartGraphProcess(FullDataFilePath);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Could not start python plot script " + e.Message + "\n" + e.StackTrace);
            Destroy(this.gameObject);
            return;
        }

        worldCreator.OnRecreateWorlds += SafeDataToFile;
    }

    private void SafeDataToFile()
    {
        SafeListDataToFile(worldCreator.sortedBrainList.Keys, FullRabbitDataFilePath);
        SafeListDataToFile(worldCreator.sortedFoxesBrainList.Keys, FullFoxDataFilePath);
    }

    private void SafeListDataToFile(IList<float> fitnessList, string file)
    {
        float avg = 0f;
        foreach (float val in fitnessList)
        {
            avg += val;
        }

        using (StreamWriter writer = File.AppendText(file))
        {
            writer.WriteLine((fitnessList[0] + ";" + (avg / fitnessList.Count) + ";" + fitnessList[fitnessList.Count - 1]).Replace(',', '.'));
            writer.Close();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        //if (showPlot)
        //{
        //    if(!rabbitGraphScript.IsRunning)
        //    {
        //        rabbitGraphScript.Run();
        //    }
        //    //StartGraphProcess(FullDataFilePath);
        //}
    }

    private void OnDestroy()
    {
        //file.Close();
    }
}

internal class GraphScript
{
    private readonly string scriptPath;
    private readonly string dataFilePath;
    private readonly float refreshInterval;

    private Process process;

    public bool IsRunning => !process.HasExited;

    public GraphScript(string scriptPath, string dataFilePath, float refreshInterval)
    {
        this.scriptPath = scriptPath;
        this.dataFilePath = dataFilePath;
        this.refreshInterval = refreshInterval;
    }

    public void Run()
    {
        process = new Process();
        process.StartInfo.FileName = scriptPath;
        process.StartInfo.Arguments = string.Format("\"{0}\" {1} \"{2}\"", scriptPath.Substring(0, scriptPath.LastIndexOf("/") + 1), refreshInterval, dataFilePath);
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
        process.Start();
    }
}