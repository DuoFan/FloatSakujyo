using EditorExtension;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameExtension
{
    public class RemoteGameConfigWindow : EditorWindow
    {
        [MenuItem("GameExtension/Windows/RemoteGameConfigWindow")]
        public static void Open()
        {
            EditorWindow.GetWindow<RemoteGameConfigWindow>();
        }

        DescriptionView descriptionView;
        RemoteGameConfig config;
        private void OnEnable()
        {
            var file = $"{Application.dataPath}/RemoteConfig.json";
            if (File.Exists(file))
            {
                var json = File.ReadAllText(file);
                config = JsonUtility.FromJson<RemoteGameConfig>(json);  
            }
            else
            {
                config = new RemoteGameConfig();
            }
            descriptionView = new DescriptionView();
            descriptionView.Set(config, true);
        }

        private void OnGUI()
        {
            descriptionView.Draw();
            if (GUILayout.Button("保存"))
            {
                Save();
            }
        }
        public void Save()
        {
            var json = JsonUtility.ToJson(config);
            var file = $"{Application.dataPath}/RemoteConfig.json";
            using (var writter = File.CreateText(file))
            {
                writter.Write(json);
                writter.Flush();
            }
            AssetDatabase.Refresh();
        }

    }
}
