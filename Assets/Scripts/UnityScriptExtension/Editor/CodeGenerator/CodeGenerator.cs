using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;

namespace EditorExtension
{
    public static class CodeGenerator
    {
        public static void GenerateClassScript(ClassBuilder classBuilder, string directory = null)
        {
            if(directory == null)
            {
                directory = $"{Application.dataPath}/Scripts";
            }
            EditorUtils.CheckOrCreateDirectory(directory);
            using (StreamWriter writer = File.CreateText($"{directory}/{classBuilder.ClassName}.cs"))
            {
                var script = classBuilder.Build();
                writer.Write(script);
                writer.Flush();
                writer.Close();
            }
        }
        public static void GenerateEnumScript(string enumClassName, string[] enumNames)
        {
            EditorUtils.CheckOrCreateScriptDirectory();
            using (StreamWriter writer = File.CreateText($"{Application.dataPath}/Scripts/{enumClassName}.cs"))
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append($"public enum {enumClassName} {{ \n");
                StringBuilder enumRename = new StringBuilder();
                for (int i = 0; i < enumNames.Length; i++)
                {
                    enumRename.Clear();
                    for (int j = 0; j < enumNames[i].Length; j++)
                    {
                        if (enumNames[i][j] == '/' || enumNames[i][j] == '-' || enumNames[i][j] == ' ')
                        {
                            enumRename.Append('_');
                        }
                        else if (j < 0 && enumNames[i][j - 1] == '/' || enumNames[i][j - 1] == '-' || enumNames[i][j - 1] == ' ')
                        {
                            enumRename.Append(enumNames[i][j]);
                        }
                    }

                    stringBuilder.Append(enumRename.ToString());
                    if (i < enumNames.Length - 1)
                    {
                        stringBuilder.Append(',');
                    }
                }
                stringBuilder.Append("}");

                writer.Write(stringBuilder.ToString());
                writer.Flush();
                writer.Close();
            }

            AssetDatabase.Refresh();
        }
    }
}
