using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorExtension
{
    public class EditorEnumPopup<T> : IDraw, IDynamicValue<T> where T : System.Enum
    {
        public T Value
        {
            get; private set;
        }
        public string Name
        {
            get; private set;
        }
        public T LastValue { get; private set; }

        public EditorEnumPopup(string name)
        {
            Name = name;
        }

        public event Action<T> OnValueChange;

        public void Draw()
        {
            SetValue((T)UnityEditor.EditorGUILayout.EnumPopup(Name, Value));
        }
        public void SetValue(T value)
        {
            if (Value.Equals(value))
            {
                return;
            }
            Value = value;
            LastValue = value;
            OnValueChange?.Invoke(value);
        }
    }
}

