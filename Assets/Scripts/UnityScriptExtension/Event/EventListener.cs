using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public abstract class EventListener : MonoBehaviour
    {
        [SerializeField,HideInInspector]
        protected string eventKey;
        public string EventKey => eventKey;
        protected abstract void RegisterEvent();
    }
}
