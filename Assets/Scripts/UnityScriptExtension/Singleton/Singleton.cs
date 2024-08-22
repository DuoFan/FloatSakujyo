using UnityEngine;

namespace GameExtension
{
    public static class Singleton
    {
        public static bool KeepWhenSingletonNull<T>(ref T singleton, T instance)
        {
            if (singleton == null)
            {
                singleton = instance;
                if (instance is MonoBehaviour mono)
                {
                    GameObject.DontDestroyOnLoad(mono.gameObject);
                }
            }
            else if (instance is MonoBehaviour mono && !singleton.Equals(instance))
            {
                mono.gameObject.SetActive(false);
            }
            return singleton.Equals(instance);
        }
    }
}