using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class Pool<T> : IClearableInTimeout
    {

        Stack<T> pool;
        public int Count => pool == null ? 0 : pool.Count;
        Func<T> ctor;
        public float LastUsedTime { get; private set; }
        public float CleanInterval { get; private set; }

        public Pool(Func<T> _ctor, float cleanInterval = -1)
        {
            ctor = _ctor;
            LastUsedTime = Time.realtimeSinceStartup;
            CleanInterval = cleanInterval;
            if(cleanInterval > 0)
            {
                TimeoutCleaner.AddElementToClean(this);
            }
        }

        ~Pool()
        {
            TimeoutCleaner.RemoveElementToClean(this);
        }

        public T Get()
        {
            if (pool == null)
                pool = new Stack<T>();

            T obj;
            if (pool.Count > 0)
                obj = pool.Pop();
            else
            {
                obj = ctor();
            }

            LastUsedTime = Time.realtimeSinceStartup;

            return obj;
        }

        public PooledObj GetPooledObj()
        {
            PooledObj pooledObj = new PooledObj();
            pooledObj.Pool = this;
            pooledObj.Obj = Get();

            LastUsedTime = Time.realtimeSinceStartup;

            return pooledObj;
        }

        public Pool<T> Return(T obj)
        {
#if UNITY_EDITOR
            if (pool.Contains(obj))
            {
                GameExtension.Logger.Error("已包含该对象");
                return this;
            }
#endif
            pool.Push(obj);
            return this;
        }

        public Pool<T> Return(PooledObj pooledObj)
        {
            pooledObj.OnReturn?.Invoke();
            pool.Push(pooledObj.Obj);
            return this;
        }

        public bool IsEmpty()
        {
            return Count <= 0;
        }

        public int Clear()
        {
            int count = Count;
            if (Count > 0)
            {
                bool isMonoBehaviour = pool.Peek() is MonoBehaviour;
                while (pool.Count > 0)
                {
                    var obj = pool.Pop();
                    if (isMonoBehaviour)
                    {
                        GameObject.Destroy((obj as MonoBehaviour).gameObject);
                    }
                }
            }
            return count;
        }

        public class PooledObj
        {
            public Pool<T> Pool { get; internal set; }
            public Action OnReturn;
            public T Obj { get; internal set; }

            public void Return()
            {
                Pool.Return(this);
            }
        }
    }
}

