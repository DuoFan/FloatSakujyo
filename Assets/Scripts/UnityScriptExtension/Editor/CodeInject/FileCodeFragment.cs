using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorExtension
{
    public class FileCodeFragment : IDraw, ISetter<(string,string)>, ISave<(string, string)>
    {
        string fileName;
        public string Code { get; private set; }
        public bool IsEnable { get; private set; }
        public void Enable()
        {
            IsEnable = true;
        }
        public void Disable()
        {
            IsEnable = false;
        }
        public void Set((string, string) input)
        {
            fileName = input.Item1;
            Code = input.Item2;
        }

        public (string, string) Save()
        {
            return (fileName, Code);
        }
        public void Draw()
        {
            EditorGUILayout.BeginVertical();
            IsEnable = EditorGUILayout.Toggle($"注入{fileName}", IsEnable);
            if (IsEnable)
            {
                EditorGUILayout.HelpBox(Code, MessageType.Info);
            }
            EditorGUILayout.EndVertical();
        }
    }
}
