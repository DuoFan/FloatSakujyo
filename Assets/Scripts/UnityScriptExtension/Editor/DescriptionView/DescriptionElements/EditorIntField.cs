using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorExtension
{
    public class EditorIntField : IDraw, IDynamicValue<int>
    {
        public string Label { get; set; }
        public int Value { get; private set; }
        public int LastValue { get; private set; }
        public event Action<int> OnValueChange;

        public EditorIntField(string label, Action<int> _onValueChange = null)
        {
            Label = label;
            OnValueChange = _onValueChange;
        }

        public void Draw()
        {
            EditorGUILayout.BeginVertical();
            Value = EditorGUILayout.IntField(Label, Value);
            if (Value != LastValue)
            {
                LastValue = Value;
                OnValueChange?.Invoke(Value);   
            }
            EditorGUILayout.EndVertical();
        }
        public void SetValue(int value)
        {
            if (Value == value)
            {
                return;
            }
            Value = value;
            LastValue = value;
            OnValueChange?.Invoke(value);
        }
    }
}
