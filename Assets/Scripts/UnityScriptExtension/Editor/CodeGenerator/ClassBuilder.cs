using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GameExtension;

namespace EditorExtension
{
    public class ClassBuilder : IBuild<string>
    {
        public string ClassName { get; private set; }
        public string nameSpace;
        public bool isAbstract;
        public AccessingModifier accessingModifier;
        public List<System.Type> interfaces;
        public List<string> usingNameSpaces;
        public AttributerBuilderManager attributerBuilderManager;
        public List<FieldBuilder> fieldBuilders;
        public List<MethodBuilder> methodBuilders;
        public void SetClassName(string className)
        {
            ClassName = className;
        }
        public void SetNameSpace(string _nameSpace)
        {
            nameSpace = _nameSpace;
        }
        public void SetAccessingModifier(AccessingModifier _accessingModifier)
        {
            accessingModifier = _accessingModifier;
        }
        public void SetAbstract(bool _isAbstract)
        {
            isAbstract = _isAbstract;
        }
        public void AddUsingNameSpace(string nameSpace)
        {
            if (usingNameSpaces == null)
            {
                usingNameSpaces = new List<string>();
            }
            usingNameSpaces.Add(nameSpace);
        }
        public void AddInterface(System.Type _interface)
        {
            if (interfaces == null)
            {
                interfaces = new List<System.Type>();
            }
            interfaces.Add(_interface);
        }
        public void AddAttributer(System.Type attributeType, params object[] parameters)
        {
            if (attributerBuilderManager == null)
            {
                attributerBuilderManager = new AttributerBuilderManager();
            }
            attributerBuilderManager.AddAttributer(attributeType, parameters);
        }
        public FieldBuilder AddField(System.Type fieldType, string fieldName, AccessingModifier accessingModifier)
        {
            return AddField(EditorUtils.GetTypeName(fieldType), fieldName, accessingModifier);
        }
        public FieldBuilder AddField(string fieldType, string fieldName, AccessingModifier accessingModifier)
        {
            if (fieldBuilders == null)
            {
                fieldBuilders = new List<FieldBuilder>();
            }
            FieldBuilder fieldBuilder = new FieldBuilder();
            fieldBuilder.fieldType = fieldType;
            fieldBuilder.SetFieldName(fieldName);
            fieldBuilder.SetAccessingModifier(accessingModifier);
            fieldBuilders.Add(fieldBuilder);
            return fieldBuilder;
        }
        public MethodBuilder AddMethod(string methodName, AccessingModifier accessingModifier)
        {
            if (methodBuilders == null)
            {
                methodBuilders = new List<MethodBuilder>();
            }
            MethodBuilder methodBuilder = new MethodBuilder();
            methodBuilder.SetMethodName(methodName);
            methodBuilder.SetAccessingModifier(accessingModifier);
            methodBuilders.Add(methodBuilder);
            return methodBuilder;
        }
        public string Build()
        {
            if (string.IsNullOrEmpty(ClassName))
            {
                throw new ParameterNullOrEmptyException("ClassName");
            }

            StringBuilder sb = new StringBuilder();
            if (usingNameSpaces != null)
            {
                for (int i = 0; i < usingNameSpaces.Count; i++)
                {
                    sb.Append($"using {usingNameSpaces[i]};");
                    sb.Append($"\n");
                }
                sb.Append($"\n");
            }

            if (!string.IsNullOrEmpty(nameSpace))
            {
                sb.Append($"namespace {nameSpace}\n{{\n");
            }

            if (attributerBuilderManager != null)
            {
                sb.Append(attributerBuilderManager.Build());
            }

            sb.Append($"{accessingModifier.ToString().ToLower()} ");

            if (isAbstract)
            {
                sb.Append("abstract ");
            }

            sb.Append($"class {ClassName} ");

            if (interfaces != null)
            {
                sb.Append(": ");
                for (int i = 0; i < interfaces.Count; i++)
                {
                    sb.Append(EditorUtils.GetTypeName(interfaces[i]));
                    if (i < interfaces.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }
            }

            sb.Append("\n{");

            if (fieldBuilders != null)
            {
                sb.Append($"\n{FieldBuilder.BuildList(fieldBuilders)}\n");
            }

            if (methodBuilders != null)
            {
                sb.Append($"\n{MethodBuilder.BuildList(methodBuilders)}\n");
            }

            sb.Append("\n}");

            if (!string.IsNullOrEmpty(nameSpace))
            {
                sb.Append("\n}");
            }

            return sb.ToString();
        }
    }

    public enum AccessingModifier
    {
        Private, Public, Protected, Internal
    }
}
