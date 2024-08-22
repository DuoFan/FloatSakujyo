using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace EditorExtension
{
    public class FieldBuilder : IBuild<string>
    {
        public string fieldType;
        public string filedName;
        public bool IsStatic;
        public AccessingModifier accessingModifier;
        public AttributerBuilderManager attributerBuilderManager;
        public void SetFieldType(System.Type type)
        {
            fieldType = EditorUtils.GetTypeName(type);
        }
        public void SetFieldName(string _filedName)
        {
            filedName = _filedName;
        }
        public void SetAccessingModifier(AccessingModifier _accessingModifier)
        {
            accessingModifier = _accessingModifier;
        }
        public void SetStatic(bool isStatic)
        {
            IsStatic = isStatic;
        }
        public void AddAttributer(System.Type attributeType, params object[] parameters)
        {
            if (attributerBuilderManager == null)
            {
                attributerBuilderManager = new AttributerBuilderManager();
            }
            attributerBuilderManager.AddAttributer(attributeType, parameters);
        }
        public virtual string Build()
        {
            if (string.IsNullOrEmpty(filedName))
            {
                throw new GameExtension.ParameterNullOrEmptyException("filedName");
            }
            string result = attributerBuilderManager == null ? string.Empty : $"{attributerBuilderManager.Build()}";
            result += $"{accessingModifier.ToString().ToLower()} ";
            result += IsStatic ? "static " : string.Empty;
            result += $"{fieldType} {filedName};";
            return result;
        }

        public static string BuildList(List<FieldBuilder> fieldBuilders)
        {
            string result = string.Empty;
            for (int i = 0; i < fieldBuilders.Count; i++)
            {
                result += $"{fieldBuilders[i].Build()}\n";
            }
            return result;
        }
    }

}