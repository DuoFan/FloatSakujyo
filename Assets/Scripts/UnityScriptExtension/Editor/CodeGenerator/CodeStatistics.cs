using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorExtension
{
    public class CodeStatistics
    {

        [MenuItem("GameExtension/Tools/CodeStatistics")]
        public static async void CodeStatisticsMenu()
        {
            string path = Application.dataPath + "/Programmer/Scripts";
            int lineCount = 0;
            var files = await EditorUtils.FindFilesAsync(path, new System.Text.RegularExpressions.Regex("\\.cs$"));
            for(int i = 0; i < files.Length; i++)
            {
                string[] lines = System.IO.File.ReadAllLines(files[i]);
                for (int j = 0; j < lines.Length; j++)
                {
                    var line = lines[j].Trim();
                    if (line.Length <= 1 || line.StartsWith("//")
                        || line.StartsWith("/*") || line.EndsWith("*/") 
                        || line.StartsWith("using") || line.StartsWith("namespace"))
                    {
                        continue;
                    }
                    lineCount++;
                }
            }
            Debug.Log($"代码文件数:{files.Length},代码总行数:{lineCount}");
        }
    }

}
