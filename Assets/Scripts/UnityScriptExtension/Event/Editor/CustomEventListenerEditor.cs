using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EditorExtension;

namespace GameExtension
{
    [CustomEditor(typeof(EventListener), true)]
    public class CustomEventListenerEditor : UnityEditor.Editor
    {
        ObjectSelector eventKeySelector;
        EventListener eventListener;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if(eventListener == null)
            {
                eventListener = target as EventListener;
            }

            if(eventKeySelector == null)
            {
                eventKeySelector = new ObjectSelector();
                var fields = typeof(EventKey).GetFields();
                foreach (var field in fields)
                {
                    if (field.IsStatic && field.IsLiteral)
                    {
                        var eventKey = field.GetValue(null).ToString();
                        eventKeySelector.AddOption(eventKey, eventKey);
                    }
                }
                eventKeySelector.SetIndex((x) => (string)x == eventListener.EventKey);
                eventKeySelector.onOptionChange = (option) =>
                {
                    EditorUtils.SetField(eventListener, "eventKey", option);
                    EditorUtility.SetDirty(eventListener);
                };
                eventKeySelector.SetName("EventKey");
            }
            eventKeySelector.Draw();
        }
    }
}

