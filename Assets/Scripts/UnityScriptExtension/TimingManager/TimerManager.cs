using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GameExtension
{
    public class TimerManager : MonoBehaviour
    {
        public static TimerManager Instance
        {
            get; private set;
        }

        //在一关内持续生效的定时器
        Queue<ITimer> levelTimers;
        //始终生效的定时器
        List<TimerObserver> alwaysTimers;
        ElapsedTimeSubject elapsedTimeSubject;
        public bool IsPausing
        {
            get; private set;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                IsPausing = true;
                Init();
                DontDestroyOnLoad(Instance);
            }
        }
        private void Update()
        {
            var deltaTime = Time.deltaTime;
            if (!IsPausing)
            {
                elapsedTimeSubject.Value = deltaTime;
            }
            for (int i = 0; i < alwaysTimers.Count; i++)
            {
                if (alwaysTimers[i].duration.TotalSeconds <= 0)
                {
                    alwaysTimers.RemoveAt(i);
                    i--;
                    continue;
                }
                alwaysTimers[i].UpdateValue(deltaTime);
            }
        }
        void Init()
        {
            levelTimers = new Queue<ITimer>();
            alwaysTimers = new List<TimerObserver>();

            elapsedTimeSubject = new ElapsedTimeSubject();
            SubjectManager.Instance.RegisterSubject(elapsedTimeSubject);
        }
        public void Resume()
        {
            IsPausing = false;
        }
        public void Pause()
        {
            IsPausing = true;
        }
        public void AddTimer(ITimer timer)
        {
            if (timer.LifeCycle == TimerLifeCycle.OneLevel)
            {
                levelTimers.Enqueue(timer);
            }
            else
            {
                var observer = new TimerObserver(timer);
                if (timer.IsAlwaysUpdate)
                {
                    alwaysTimers.Add(observer);
                }
                else
                {
                    SubjectManager.Instance.RegisterObserver(observer);
                }
            }
        }
        
        /// <summary>
        /// 添加计划表
        /// </summary>
        /// <param name="action">完成时的回调</param>
        /// <param name="delayTime"></param>
        /// <param name="isSubElaspedTimeWhenGameResume"></param>
        /// <param name="onElaspedTime"></param>
        /// <param name="updateInterval"></param>
        /// <param name="timerLifeCycle"></param>
        /// <param name="isAlwaysUpdate"></param>
        /// <returns></returns>
        public Schedule AddSchedule(Action action, float delayTime, bool isSubElaspedTimeWhenGameResume = false, Action<float> onElaspedTime = null,
            float updateInterval = 0, TimerLifeCycle timerLifeCycle = TimerLifeCycle.OneLevelInDuration, bool isAlwaysUpdate = false)
        {
            var schedule = new Schedule(action, delayTime, updateInterval);
            schedule.LifeCycle = timerLifeCycle;
            schedule.IsAlwaysUpdate = isAlwaysUpdate;
            schedule.IsSubElaspedTimeWhenGameResume = isSubElaspedTimeWhenGameResume;
            schedule.OnElaspedTime += onElaspedTime;
            AddTimer(schedule);
            return schedule;
        }
        public CountDown AddCountDown(Action<float> onUpdate, Action onTimeout, float countDown,
            bool isSubElaspedTimeWhenGameResume = false, Action<float> onElaspedTime = null,
            float updateInterval = 0, TimerLifeCycle timerLifeCycle = TimerLifeCycle.OneLevelInDuration, bool isAlwaysUpdate = false)
        {
            var countDownTimer = new CountDown(onUpdate, onTimeout, countDown, updateInterval);
            countDownTimer.LifeCycle = timerLifeCycle;
            countDownTimer.IsAlwaysUpdate = isAlwaysUpdate;
            countDownTimer.IsSubElaspedTimeWhenGameResume = isSubElaspedTimeWhenGameResume;
            countDownTimer.OnElaspedTime += onElaspedTime;
            AddTimer(countDownTimer);
            return countDownTimer;
        }

        void RemoveTimerObserver(TimerObserver timerObserver)
        {
            SubjectManager.Instance.RemoveObserver(timerObserver);
        }
        public void RemoveTimerNotTimeout(ITimer timer)
        {
            if (timer.IsAlwaysUpdate)
            {
                for (int i = 0; i < alwaysTimers.Count; i++)
                {
                    var _timer = alwaysTimers[i];
                    if (_timer.timer == timer)
                    {
                        alwaysTimers.RemoveAt(i);
                        break;
                    }
                }
            }
            else
            {
                for (int i = elapsedTimeSubject.Observers.Count - 1; i >= 0; i--)
                {
                    var _timer = elapsedTimeSubject.Observers[i];
                    if (_timer is TimerObserver observer)
                    {
                        if (observer.timer == timer)
                        {
                            SubjectManager.Instance.RemoveObserver(observer);
                            break;
                        }
                    }
                }
            }
        }

        //清除一关内的定时器，并取消对应效果
        public void ClearLevelTiming()
        {
            while (levelTimers.Count > 0)
            {
                levelTimers.Dequeue().Timeout();
            }

            for (int i = elapsedTimeSubject.Observers.Count - 1; i >= 0; i--)
            {
                //有可能在timing超时的时候将其他的timing移除了，导致i >= elapsedTimeSubject.Observers.Count
                if (i >= elapsedTimeSubject.Observers.Count)
                {
                    continue;
                }

                var observer = elapsedTimeSubject.Observers[i] as TimerObserver;
                if (observer.timer.LifeCycle == TimerLifeCycle.OneLevelInDuration)
                {
                    observer.timer.Timeout();
                    elapsedTimeSubject.Observers.RemoveAt(i);
                }
            }

            for (int i = alwaysTimers.Count - 1; i >= 0; i--)
            {
                //有可能在timing超时的时候将其他的timing移除了，导致i >= alwaysTimers.Count
                if (i >= alwaysTimers.Count)
                {
                    continue;
                }

                var observer = alwaysTimers[i];
                if (observer.timer.LifeCycle == TimerLifeCycle.OneLevelInDuration)
                {
                    observer.timer.Timeout();
                    alwaysTimers.RemoveAt(i);
                }
            }
        }

        DateTime lastFocusTime;
        bool applicationPause;

        private void OnApplicationPause(bool pause)
        {
            if(Application.platform == RuntimePlatform.WindowsEditor)
            {
                return;
            }

            if (!pause && applicationPause)
            {
                applicationPause = false;

                var elaspedTime = (float)(DateTime.Now - lastFocusTime).TotalSeconds;

                if (!IsPausing)
                {
                    for (int i = elapsedTimeSubject.Observers.Count - 1; i >= 0; i--)
                    {
                        var observer = elapsedTimeSubject.Observers[i] as TimerObserver;
                        observer.OnElaspedTime(elaspedTime);
                    }
                }

                for (int i = alwaysTimers.Count - 1; i >= 0; i--)
                {
                    var observer = alwaysTimers[i];
                    observer.OnElaspedTime(elaspedTime);
                }
            }
            else if(pause && !applicationPause)
            {
                applicationPause = true;
                lastFocusTime = DateTime.Now;
            }
        }

        //时间主题
        class ElapsedTimeSubject : ISubject<float>
        {
            float value;
            public float Value
            {
                get => value;
                set
                {
                    this.value = value;
                    SubjectManager.Instance.NotifyObservers(this);
                }
            }
            public List<IObserver<ISubject<float>, float>> Observers { get; set; }
        }
        class TimerObserver : IObserver<ElapsedTimeSubject, float>
        {
            public ITimer timer;
            public TimeSpan duration;
            float accumulatedTime;
            public TimerObserver(ITimer _timer)
            {
                timer = _timer;
                timer.OnTimeout += OnTimeout;
                duration = TimeSpan.FromSeconds(timer.Duration);
            }
            public void UpdateValue(float value)
            {
                if (timer.IsPause)
                {
                    return;
                }

                try
                {
                    duration = duration.Subtract(TimeSpan.FromSeconds(value));
                    accumulatedTime += value;
                    if (accumulatedTime >= timer.UpdateInterval || duration.TotalSeconds <= 0)
                    {
                        accumulatedTime = 0;
                        timer.UpdateDuration((float)duration.TotalSeconds);
                        if (duration.TotalSeconds <= 0)
                        {
                            timer.Timeout();
                        }
                    }
                }
                catch (Exception e)
                {
                    GameExtension.Logger.Exception($"发生以下问题导致定时器无法运行:{e.Message}");
                    OnTimeout();
                }
            }
            public void OnElaspedTime(float value)
            {
                if (timer.IsPause)
                {
                    return;
                }

                if (timer.IsSubElaspedTimeWhenGameResume)
                {
                    duration = duration.Subtract(TimeSpan.FromSeconds(value));
                }
                timer.HandleElaspedTime(value);
            }
            void OnTimeout()
            {
                Instance.RemoveTimerObserver(this);
                timer.OnTimeout -= OnTimeout;
            }
        }
    }
    public class Schedule : ITimer
    {
        public Schedule(Action onCompleted, float delayTime, float updateInterval)
        {
            LifeCycle = TimerLifeCycle.OneLevelInDuration;
            Duration = delayTime;
            OnCompleted += onCompleted;
            UpdateInterval = updateInterval;
        }
        public TimerLifeCycle LifeCycle { get; set; }
        public float Duration { get; set; }
        public float UpdateInterval { get; set; }

        public bool IsAlwaysUpdate { get; set; }
        public bool IsPause { get; set; }

        public event Action<float> OnUpdate;
        public event Action OnCompleted;
        public event Action OnTimeout;
        public bool IsSubElaspedTimeWhenGameResume { get; set; }
        public event Action<float> OnElaspedTime;

        public void UpdateDuration(float duration)
        {
            OnUpdate?.Invoke(duration);
            //倒计时结束
            if (duration <= 0)
            {
                OnCompleted?.Invoke();
            }
        }
        public void Timeout()
        {
            OnTimeout?.Invoke();
        }
        public void HandleElaspedTime(float elaspedTime)
        {
            OnElaspedTime?.Invoke(elaspedTime);
        }
    }
    public class CountDown : ITimer
    {
        public CountDown(Action<float> onUpdate, Action onTimeout, float countDown, float updateInterval)
        {
            LifeCycle = TimerLifeCycle.OneLevelInDuration;
            Duration = countDown;
            UpdateInterval = updateInterval;
            OnUpdate += onUpdate;
            OnTimeout += onTimeout;
        }
        public TimerLifeCycle LifeCycle { get; set; }
        public float Duration { get; set; }

        public float UpdateInterval { get; set; }
        public bool IsAlwaysUpdate { get; set; }

        public event Action<float> OnUpdate;
        public event Action OnTimeout;
        public bool IsSubElaspedTimeWhenGameResume { get; set; }
        public event Action<float> OnElaspedTime;
        public bool IsPause { get; set; }

        public void UpdateDuration(float duration)
        {
            OnUpdate?.Invoke(duration);
        }
        public void Timeout()
        {
            OnTimeout?.Invoke();
        }
        public void HandleElaspedTime(float elaspedTime)
        {
            OnElaspedTime?.Invoke(elaspedTime);
        }
    }
}

