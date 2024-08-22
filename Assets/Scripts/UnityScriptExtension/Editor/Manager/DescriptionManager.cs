using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorExtension
{
    public class DescriptionManager : IDraw,ISetter<string>,ISave<string>
    {
        public string description;
        public void Draw()
        {
            description = EditorGUILayout.TextField(description);
        }
        public void Set(string obj)
        {
            description = obj;
        }
        public string Save() => description;
    }
}

