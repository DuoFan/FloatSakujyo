using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorExtension
{
    public class ExhibitWindow : EditorWindow
    {
        Node node;
        public static ExhibitWindow Draw(Node node)
        {
            var window = EditorWindow.CreateInstance<ExhibitWindow>();
            window.node = node;
            window.Show();
            return window;
        }

        private void OnGUI()
        {
            node.NodeContent.Draw();
        }

        private void OnInspectorUpdate()
        {
            node.Update();
        }
    }
}

