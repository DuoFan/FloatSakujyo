using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GameExtension
{
    [CustomEditor(typeof(ScrollList))]
    public class CustomScrollListEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
