using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public interface IRecyclable 
    {
        public string PoolAddress
        {
            get; set;
        }
        public GameObject GameObject { get; }
        public void OnCreate();
    }
}
