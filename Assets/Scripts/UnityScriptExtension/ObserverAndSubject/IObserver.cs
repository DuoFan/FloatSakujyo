using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public interface IObserver<out T1, T2> where T1 : ISubject<T2>
    {
        void UpdateValue(T2 value);
    }
}
