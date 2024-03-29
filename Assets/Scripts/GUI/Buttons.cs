﻿using SFB;
using UnityEngine;
using World;

public class Buttons : MonoBehaviour
{
    public WorldCreator worldCreator;

    public void OnSaveRabbitButton()
    {
        try
        {
            var extensions = new[] {
            new ExtensionFilter("json", "json")
            };
            string path = StandaloneFileBrowser.SaveFilePanel("Choose file to safe", "", "", extensions);

            if (path.Length != 0)
            {
                worldCreator.SaveRabbitBrainToFile(path);
            }
        }
        catch (System.Exception e)
        {
        }
    }

    public void OnLoadRabbitButton()
    {
        try
        {
            var extensions = new[] {
            new ExtensionFilter("json", "json")
            };
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Choose file to load", "", extensions, false);

            string path = paths[0];
            if (path.Length != 0)
            {
                worldCreator.LoadRabbitBrainFromFile(path);
            }
        }
        catch (System.Exception)
        {
        }
    }

    public void OnSaveFoxButton()
    {
        try
        {
            var extensions = new[] {
            new ExtensionFilter("json", "json")
            };
            string path = StandaloneFileBrowser.SaveFilePanel("Choose file to safe", "", "", extensions);

            if (path.Length != 0)
            {
                worldCreator.SaveFoxBrainToFile(path);
            }
        }
        catch (System.Exception)
        {
        }
    }

    public void OnLoadFoxButton()
    {
        try
        {
            var extensions = new[] {
            new ExtensionFilter("json", "json")
            };
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Choose file to load", "", extensions, false);

            string path = paths[0];
            if (path.Length != 0)
            {
                worldCreator.LoadFoxBrainFromFile(path);
            }
        }
        catch (System.Exception)
        {
        }
    }

    public void OnQuitButton()
    {
        print("Quit");
        worldCreator.DestroyAllWorlds();
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}