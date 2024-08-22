using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class SingletonBase<T>
    {
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    return default;
                }
                return instance.Cast<T>();
            }
        }

        protected static SingletonBase<T> instance;

        protected bool TryKeepThisAsSingleton()
        {
            return Singleton.KeepWhenSingletonNull(ref instance, this);
        }
    }
}

