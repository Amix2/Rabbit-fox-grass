using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using World;

public class Buttons : MonoBehaviour
{
    public WorldCreator worldCreator;

    public void OnSaveRabbitButton()
    {
        string path = EditorUtility.SaveFilePanel("Choose file to load", "", "", "txt");
        if (path.Length != 0)
        {
            worldCreator.SaveRabbitBrainToFile(path);
        }
    }

    public void OnLoadRabbitButton()
    {

        string path = EditorUtility.OpenFilePanel("Choose file to load", "", "txt");
        if (path.Length != 0)
        {
            worldCreator.LoadRabbitBrainFromFile(path);
        }
    }

    public void OnSaveFoxButton()
    {
        string path = EditorUtility.SaveFilePanel("Choose file to load", "", "", "txt");
        if (path.Length != 0)
        {
            worldCreator.SaveFoxBrainToFile(path);
        }
    }

    public void OnLoadFoxButton()
    {

        string path = EditorUtility.OpenFilePanel("Choose file to load", "", "txt");
        if (path.Length != 0)
        {
            worldCreator.LoadFoxBrainFromFile(path);
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
