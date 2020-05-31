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

    private string PlotDataPath => Application.dataPath + "/../Plots/";
    private string PlotScriptPath => Application.dataPath + "/Plots/";
    private string sessionFileName;
    private string FullFilePath => PlotDataPath + sessionFileName;

    private string pythonScriptName = "runPlot.bat";

    private Process cmd;
    // FileStream file;

    // Start is called before the first frame update
    private void Start()
    {
        if (!showPlot || !Settings.World.collectHistory)
        {
            Destroy(this.gameObject);
            return;
        }

        try
        {
            sessionFileName = DateTime.Now.ToShortDateString().Replace("/", "-") + "-" +
                              DateTime.Now.ToLongTimeString().Replace(":", "-") +
                              ".txt"; // DateTime.Now.ToShortDateString().Replace("/", ":") + "-" + DateTime.Now.ToLongTimeString()+".txt";

            File.Create(FullFilePath).Close();
            using (StreamWriter writer = File.AppendText(FullFilePath))
            {
                writer.WriteLine("Max;Avg;Min");
                writer.Close();
            }

            StartGraphProcess();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Could not start python plot script " + e.Message + "\n" + e.StackTrace);
            Destroy(this.gameObject);
            return;
        }

        worldCreator.OnRecreateWorlds += SafeDataToFile;
    }

    private void StartGraphProcess()
    {
        cmd = new Process();
        cmd.StartInfo.FileName = PlotScriptPath + pythonScriptName;
        cmd.StartInfo.Arguments = string.Format("\"{0}\" {1} \"{2}\"", PlotScriptPath, plotRefreshTime, FullFilePath);
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = true;
        cmd.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
        cmd.Start();
    }

    private void SafeDataToFile()
    {
        IList<float> fitnessList = worldCreator.sortedBrainList.Keys;
        float avg = 0f;
        foreach (float val in fitnessList)
        {
            avg += val;
        }

        using (StreamWriter writer = File.AppendText(FullFilePath))
        {
            writer.WriteLine((fitnessList[0] +";" + (avg / fitnessList.Count) + ";" + fitnessList[fitnessList.Count - 1]).Replace(',','.'));
            writer.Close();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (showPlot && cmd.HasExited)
        {
            StartGraphProcess();
        }
    }

    private void OnDestroy()
    {
        //file.Close();
    }
}