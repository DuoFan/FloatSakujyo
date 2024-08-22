using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class DebugLog : ILog
    {
        public void Log(string info)
        {
            Debug.Log(info);
        }
    }
}
