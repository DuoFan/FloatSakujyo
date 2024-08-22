using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorExtension
{
    public class EditorFloatField : IDraw, IDynamicValue<float>
    {
        public string Label { get; set; }
        public float Value { get; private set; }
        public float LastValue { get; private set; }

        public event Action<float> OnValueChange;

        public EditorFloatField(string label, Action<float> _onValueChange = null)
        {
            Label = label;
            OnValueChange = _onValueChange;
        }

        public void Draw()
        {
            EditorGUILayout.BeginVertical();
            Value = EditorGUILayout.FloatField(Label, Value);
            if (Value != LastValue)
            {
                LastValue = Value;
                OnValueChange?.Invoke(Value);
            }
            EditorGUILayout.EndVertical();
        }
        public void SetValue(float value)
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
