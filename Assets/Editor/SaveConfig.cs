using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Settings))]
public class SaveConfig : Editor
{
    public override void OnInspectorGUI()
    {
        Settings myTarget = (Settings)target;

        if (GUILayout.Button("Save config"))
        {
            SettingsSerialized set = new SettingsSerialized(myTarget);
            Debug.Log("Save config file to " + SettingsSerialized.Path);

            string json = JsonConvert.SerializeObject(set, Formatting.Indented);
            //File.Create(ConfigFilePath).Close();
            File.WriteAllText(SettingsSerialized.Path, json);
            
        }
        DrawDefaultInspector();
    }
}
