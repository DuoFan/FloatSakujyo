using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public enum TimerLifeCycle
    {
        //持续全局，在时间结束前不会消除
        Global,
        //持续一整关,在关卡结束前不会消除
        OneLevel,
        //持续一整关，当时间结束或关卡结束时消除
        OneLevelInDuration
    }
}

