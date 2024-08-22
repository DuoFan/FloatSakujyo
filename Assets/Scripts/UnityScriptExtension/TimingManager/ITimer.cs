using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    /// <summary>
    /// 时间接口,单位秒,时间结束自动取消
    /// </summary>
    public interface ITimer
    {
        TimerLifeCycle LifeCycle { get; }
        //单位秒
        float Duration { get; }
        float UpdateInterval { get; }
        bool IsAlwaysUpdate { get; }
        bool IsPause { set; get; }
        event Action<float> OnUpdate;
        void UpdateDuration(float duration);
        event Action OnTimeout;
        void Timeout();
        bool IsSubElaspedTimeWhenGameResume { get; set; }
        event Action<float> OnElaspedTime;  
        public void HandleElaspedTime(float elaspedTime);
    }
}
