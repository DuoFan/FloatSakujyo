using DG.Tweening.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class DynamicDataProxy<T> : ISetter<T>, IGetter<T>
    {
        private readonly Action<T> setter;
        private readonly Func<T> getter;

        public event Action<T> OnValueChange;
        public DynamicDataProxy(Action<T> _setter, Func<T> _getter)
        {
            setter = _setter ?? throw new ArgumentNullException(nameof(_setter));
            getter = _getter ?? throw new ArgumentNullException(nameof(_getter));
        }
        public void Set(T obj)
        {
            if(obj.Equals(Get()))
            {
                return;
            }
            setter(obj);
            OnValueChange?.Invoke(obj);
        }
        public T Get()
        {
            return getter();
        }
        public static implicit operator T(DynamicDataProxy<T> proxy) { return proxy.Get(); }
        public static DynamicDataProxy<T> CreateWithDefaultDataProvider()
        {
            T dataProvider = default;
            DynamicDataProxy<T> dynamicDataProxy = new DynamicDataProxy<T>((v) =>
            {
                dataProvider = v;
            }, () => dataProvider);
            return dynamicDataProxy;
        }
        public override string ToString()
        {
            return Get()?.ToString() ?? throw new ArgumentNullException(nameof(getter));
        }
    }
}
