using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class Logger : MonoBehaviour
    {
        static Logger instance;
        static ILog[] logs;
        public static void Log(string log)
        {
            CheckInstance();
            for (int i = 0; i < logs.Length; i++)
            {
                logs[i].Log($"{System.DateTime.Now}---Log:{log}");
            }
        }
        public static void Warning(string warning)
        {
            CheckInstance();
            for (int i = 0; i < logs.Length; i++)
            {
                logs[i].Log($"{System.DateTime.Now}---Warning:{warning}");
            }
        }
        public static void Exception(string exception)
        {
            CheckInstance();
            for (int i = 0; i < logs.Length; i++)
            {
                logs[i].Log($"{System.DateTime.Now}---Exception:{exception}");
            }
        }
        public static void Error(string error)
        {
            CheckInstance();
            for (int i = 0; i < logs.Length; i++)
            {
                logs[i].Log($"{System.DateTime.Now}---Error:{error}");
            }
        }
        static void CheckInstance()
        {
            if (instance == null)
            {
                var go = new GameObject("Logger");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<Logger>();
                List<ILog> _logs = new List<ILog>();
                _logs.Add(new DebugLog());
#if FILE_LOG
                _logs.Add(new FileLog());
#endif
                logs = _logs.ToArray();
                for (int i = 0; i < logs.Length; i++)
                {
                    logs[i].Log($"\n{System.DateTime.Now}---开始记录");
                }
            }
        }
        private void OnApplicationQuit()
        {
            for (int i = 0; i < logs.Length; i++)
            {
                var log = logs[i] as IOnApplicationQuit;
                log?.OnApplicationQuit();
            }
        }
    }
}
