using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace GameExtension
{
    public class FileLog : ILog,IOnApplicationQuit
    {
        StreamWriter sw;
        void Init()
        {
            var path = $"{Application.persistentDataPath}/{Application.productName}.txt";
            if (!File.Exists(path))
            {
                sw = File.CreateText(path);
            }
            else
            {
                sw = new StreamWriter(path,true);
            }
        }
        public void Log(string info)
        {
            if(sw == null)
            {
                Init();
            }
            sw.WriteLine(info);
            sw.Flush();
        }

        public void OnApplicationQuit()
        {
            sw.Close();
        }
    }
}
