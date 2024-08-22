using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using GameExtension;

namespace EditorExtension
{
    public static class DescriptionElementFactory 
    {
        public static IDraw GetElement(object obj,PropertyInfo propertyInfo,string label)
        {
            if (propertyInfo.PropertyType.Equals(typeof(bool)))
            {
                EditorToggle editorToggle = new EditorToggle(label, null,i =>
                {
                    propertyInfo.SetValue(obj, i);    
                });
                editorToggle.SetValue((bool)propertyInfo.GetValue(obj));
                return editorToggle;
            }

            if (propertyInfo.PropertyType.Equals(typeof(int)))
            {
                EditorIntField editorIntField = new EditorIntField(label, i =>
                {
                    propertyInfo.SetValue(obj, i);
                });
                editorIntField.SetValue((int)propertyInfo.GetValue(obj));
                return editorIntField;
            }

            if (propertyInfo.PropertyType.Equals(typeof(float)))
            {
                EditorFloatField editorFloatField = new EditorFloatField(label, i =>
                {
                    propertyInfo.SetValue(obj, i);
                });
                editorFloatField.SetValue((float)propertyInfo.GetValue(obj));
                return editorFloatField;
            }

            return default;
        }
        public static IDraw GetElement(object obj, FieldInfo fieldInfo, string label)
        {
            if (fieldInfo.FieldType.Equals(typeof(bool)))
            {
                EditorToggle editorToggle = new EditorToggle(label, null, i =>
                {
                    fieldInfo.SetValue(obj, i);
                });
                editorToggle.SetValue((bool)fieldInfo.GetValue(obj));
                return editorToggle;
            }

            if (fieldInfo.FieldType.Equals(typeof(int)))
            {
                EditorIntField editorIntField = new EditorIntField(label, i =>
                {
                    fieldInfo.SetValue(obj, i);
                });
                editorIntField.SetValue((int)fieldInfo.GetValue(obj));
                return editorIntField;
            }

            if (fieldInfo.FieldType.Equals(typeof(float)))
            {
                EditorFloatField editorFloatField = new EditorFloatField(label, i =>
                {
                    fieldInfo.SetValue(obj, i);
                });
                editorFloatField.SetValue((float)fieldInfo.GetValue(obj));
                return editorFloatField;
            }

            return default;
        }
    }
}

