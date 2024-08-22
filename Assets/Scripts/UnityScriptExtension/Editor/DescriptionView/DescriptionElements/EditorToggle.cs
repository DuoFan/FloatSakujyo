using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorExtension
{
    public class EditorToggle : IDraw,IDynamicValue<bool>
    {
        public string Label { get; set; }
        public bool Value { get; private set; }
        public bool LastValue { get; private set; }

        public event System.Action drawWhenTrue;
        public event Action<bool> OnValueChange;

        public EditorToggle(string label, Action _drawWhenTrue = null, Action<bool> onValueChange = null)
        {
            Label = label;
            drawWhenTrue = _drawWhenTrue;
            OnValueChange = onValueChange;
        }

        public void Draw()
        {
            Value = EditorGUILayout.BeginToggleGroup(Label, Value);
            if (Value != LastValue)
            {
                LastValue = Value;
                OnValueChange?.Invoke(Value);
            }
            if (Value)
            {
                drawWhenTrue?.Invoke();
            }
            EditorGUILayout.EndToggleGroup();
        }
        public void SetValue(bool value)
        {
            if(Value == value)
            {
                return;
            }
            Value = value;
            LastValue = value;
            OnValueChange?.Invoke(value);
        }   
    }

}
