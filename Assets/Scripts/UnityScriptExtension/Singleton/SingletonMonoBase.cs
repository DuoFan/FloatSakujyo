using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class SingletonMonoBase<T> : MonoBehaviour
    {
        public static T Instance
        {
            get
            {
                if(instance == null)
                {
                    return default;
                }
                return instance.Cast<T>();
            }
        }

        private static SingletonMonoBase<T> instance;

        protected virtual void Awake()
        {
            TryKeepThisAsSingleton();
        }

        protected bool TryKeepThisAsSingleton()
        {
            return Singleton.KeepWhenSingletonNull(ref instance, this);
        }
    }
}

