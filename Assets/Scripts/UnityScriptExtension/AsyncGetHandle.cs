using System;
using System.Collections;
using UnityEngine;

namespace GameExtension
{
    public class AsyncGetHandle<T> : CustomYieldInstruction
    {
        public T Object { get; private set; }
        event Action<T> completed;

        public event Action<T> Completed
        {
            add
            {
                if (IsDone) value(Object);
                else completed += value;
            }
            remove { completed -= value; }
        }

        public bool IsDone => Object != null;
        public override bool keepWaiting => Object == null;

        public void SetResult(T obj)
        {
            Object = obj;
            completed?.Invoke(obj);
        }

        public IEnumerator WaitForCompletion()
        {
            yield return new WaitUntil(() => Object != null);
        }
    }
}