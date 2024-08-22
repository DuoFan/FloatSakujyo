using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameExtension
{
    public interface ISubject<T>
    {
        T Value { get; set; }
        List<IObserver<ISubject<T>, T>> Observers
        {
            get; set;
        }
    }
    //开关主题，用来指示某个类型的开关是否被打开
    public class SwitchSubject<T> : ISubject<bool>
    {
        public bool Value
        {
            get => switchValue;
            set
            {
                if(this.switchValue != value)
                {
                    this.switchValue = value;
                    SubjectManager.Instance.NotifyObservers(this);
                }
            }
        }
        bool switchValue;
        public List<IObserver<ISubject<bool>, bool>> Observers { get; set; }
    }

    public class SwitchObserver<T> : IObserver<SwitchSubject<T>, bool>
    {
        public event Action<bool> OnSwitchChange;

        public SwitchObserver(Action<bool> onSwitchChange)
        {
            OnSwitchChange = onSwitchChange;
        }

        public void UpdateValue(bool switchState)
        {
            OnSwitchChange?.Invoke(switchState); 
        }
    }
}
