using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GameExtension
{
    public class ClickCallback : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent _event;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.dragging)
            {
                return;
            }
            _event.Invoke();
        }
    }
}
