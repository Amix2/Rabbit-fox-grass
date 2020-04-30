using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using World;

public class LoadSaveButtons : MonoBehaviour
{
    public WorldCreator worldCreator;

    public void OnSaveButton()
    {
        string path = EditorUtility.SaveFilePanel("Choose file to load", "", "", "txt");
        if (path.Length != 0)
        {
            worldCreator.SaveBestBrainToFile(path);
        }
    }

    public void OnLoadButton()
    {

        string path = EditorUtility.OpenFilePanel("Choose file to load", "", "txt");
        if (path.Length != 0)
        {
            worldCreator.LoadBrainFromFile(path);
        }
    }
}
