using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class SubjectManager : SingletonMonoBase<SubjectManager>
    {
        Dictionary<Type, object> taskSubjects;
        protected override void Awake()
        {
            if(TryKeepThisAsSingleton())
            {
                taskSubjects = new Dictionary<Type, object>();
            }
        }
        public void RegisterSubject<T>(ISubject<T> subject)
        {
            taskSubjects.Add(subject.GetType(), subject);
            subject.Observers = new List<IObserver<ISubject<T>, T>>();
        }
        public void RemoveSubject<T>(ISubject<T> subject)
        {
            taskSubjects.Remove(subject.GetType());
            subject.Observers = null;
        }
        public void NotifyObservers<T>(ISubject<T> subject)
        {
            if(subject.Observers == null)
            {
                return;
            }

            for (int i = subject.Observers.Count - 1; i >= 0; i--)
            {
                subject.Observers[i].UpdateValue(subject.Value);
            }
        }
        public void RegisterObserver<Subject, ValueType>(IObserver<Subject, ValueType> observer)
            where Subject : class, ISubject<ValueType>
        {
            var subject = taskSubjects[typeof(Subject)] as Subject;
            subject.Observers.Add(observer);
            observer.UpdateValue(subject.Value);
        }
        //切换关卡时不需要显式移除关卡内的观察者，因为观察者依赖于主题，
        //而主题被移除后这些观察者也就失效了
        public void RemoveObserver<Subject, ValueType>(IObserver<Subject, ValueType> observer)
            where Subject : class, ISubject<ValueType>
        {
            var subject = taskSubjects[typeof(Subject)] as Subject;
            subject.Observers.Remove(observer);
        }
    }
}
