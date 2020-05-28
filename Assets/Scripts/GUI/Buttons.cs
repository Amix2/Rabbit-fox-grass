using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using World;

public class Buttons : MonoBehaviour
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

    public void OnQuitButton()
    {
        print("Quit");
        worldCreator.GetComponent<WorldCreator>().DestroyAllWorlds();
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
