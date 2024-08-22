using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public interface ISave<T>
    {
        T Save();
    }
    public interface ISetter<T>
    {
        void Set(T obj);
    }
    public interface IGetter<T>
    {
        T Get();
    }
    public interface IOnApplicationQuit
    {
        void OnApplicationQuit();
    }
}
