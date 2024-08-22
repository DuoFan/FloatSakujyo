using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class CoroutineManager : MonoBehaviour
    {
        public static CoroutineManager Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new GameObject("CoroutineManager").AddComponent<CoroutineManager>();
                    DontDestroyOnLoad(instance.gameObject);
                }
                return instance;
            }
        }
        static CoroutineManager instance;
    }
}
