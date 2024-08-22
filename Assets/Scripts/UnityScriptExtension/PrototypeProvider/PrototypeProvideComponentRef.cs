using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class PrototypeProvideComponentRef : MonoBehaviour
    {
        [SerializeField]
        MonoBehaviour componentRef;
        public MonoBehaviour ComponentRef => componentRef;
    }
}

