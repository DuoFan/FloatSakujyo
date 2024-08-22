using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GameExtension
{
    public partial class EventManager
    {
        static EventManager instance;
        public static EventManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EventManager();
                    instance.eventComparerByPriority = new EventComparerByPriority();   
                }
                return instance;
            }
        }

        EventComparerByPriority eventComparerByPriority;

        private Dictionary<string, SortedList<int, Delegate>> events = new Dictionary<string, SortedList<int, Delegate>>();

        /// <summary>
        /// 不要滥用，该挂载对象事件就挂载对象事件
        /// </summary>
        public void RegisterEvent<T>(string eventKey, EventHandler<T> handler,int priority = 0)
        {
            if (!events.TryGetValue(eventKey, out SortedList<int, Delegate> delegates))
            {
                delegates = new SortedList<int, Delegate>(eventComparerByPriority);
                events[eventKey] = delegates;
            }

            if(!delegates.TryGetValue(priority,out Delegate _delegate))
            {
                _delegate = handler;
            }
            else
            {
                _delegate = Delegate.Combine(_delegate, handler);
            }
            delegates[priority] = _delegate;
        }

        public void RemoveEvent<T>(string eventKey, EventHandler<T> handler,int priority = 0)
        {
            if (events.TryGetValue(eventKey, out SortedList<int, Delegate> delegates))
            {
                if(delegates.TryGetValue(priority,out Delegate _delegate))
                {
                    _delegate = Delegate.Remove(_delegate, handler);
                    if(_delegate == null)
                    {
                        delegates.Remove(priority);
                        delegates.TrimExcess();
                        if (delegates.Count == 0)
                        {
                            events.Remove(eventKey);
                            events.TrimExcess();
                        }
                    }
                    else
                    {
                        delegates[priority] = _delegate;
                    }
                }
            }
        }

        public void ClearEvent(string eventKey)
        {
            events.Remove(eventKey);
        }

        public void InvokeEvent<T>(string eventKey, object sender, T context)
        {
            if (events.TryGetValue(eventKey, out SortedList<int, Delegate> delegates))
            {
                var _delegates = delegates.Values;
                for (int i = 0; i < _delegates.Count; i++)
                {
                    (_delegates[i] as EventHandler<T>).Invoke(sender, context);
                }
            }
        }

        class EventComparerByPriority : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                return -(x - y);
            }
        }
    }
}
