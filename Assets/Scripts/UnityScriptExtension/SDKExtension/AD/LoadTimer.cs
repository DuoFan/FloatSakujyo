using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDKExtension
{
    public class LoadTimer
    {
        public float loadTime = 0.2f;
        public float loadTimeHf = 2F;
        const float MAX_LOAD_TIME = 10;
        public void Update()
        {
            if (loadTime < MAX_LOAD_TIME)
                loadTime *= 2;
            else
                loadTime += loadTimeHf / 10;
        }
        public void Stop()
        {
            loadTimeHf = loadTime / 2;
            loadTime = 0.2f;
        }
    }
}
