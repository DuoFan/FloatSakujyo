using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

namespace EditorExtension
{
    public class MethodBuilder : IBuild<string>
    {
        public bool isStatic;
        public string returnType;
        public string methodName;
        public AccessingModifier accessingModifier;
        public AttributerBuilderManager attributerBuilderManager;
        public List<ParameterBuilder> parameterBuilders;
        public CodeBuilder codeBuilder = new CodeBuilder();
        public void SetReturnType(System.Type type)
        {
            returnType = EditorUtils.GetTypeName(type);
        }
        public void SetMethodName(string _methodName)
        {
            methodName = _methodName;
        }
        public void SetAccessingModifier(AccessingModifier _accessingModifier)
        {
            accessingModifier = _accessingModifier;
        }
        public void AddAttributer(System.Type attributeType, params object[] parameters)
        {
            if (attributerBuilderManager == null)
            {
                attributerBuilderManager = new AttributerBuilderManager();
            }
            attributerBuilderManager.AddAttributer(attributeType, parameters);
        }
        public ParameterBuilder AddParameter(System.Type parameterType, string parameterName, ParameterModifier parameterModifier)
        {
            return AddParameter(EditorUtils.GetTypeName(parameterType), parameterName, parameterModifier);
        }
        public ParameterBuilder AddParameter(string parameterType, string parameterName, ParameterModifier parameterModifier)
        {
            if (parameterBuilders == null)
            {
                parameterBuilders = new List<ParameterBuilder>();
            }
            ParameterBuilder parameterBuilder = new ParameterBuilder();
            parameterBuilder.parameterType = parameterType;
            parameterBuilder.SetParameterName(parameterName);
            parameterBuilder.SetParameterModifier(parameterModifier);
            parameterBuilders.Add(parameterBuilder);
            return parameterBuilder;
        }
        public string Build()
        {
            if (string.IsNullOrEmpty(methodName))
            {
                throw new GameExtension.ParameterNullOrEmptyException("methodName");
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(attributerBuilderManager == null ? string.Empty : $"{attributerBuilderManager.Build()}");
            sb.Append($"{accessingModifier.ToString().ToLower()} ");
            sb.Append(isStatic? "static " : string.Empty);
            sb.Append(returnType == null ? "void " : $"{returnType} ");
            sb.Append(methodName);
            sb.Append("(");
            if (parameterBuilders != null)
            {
                sb.Append(ParameterBuilder.BuildeList(parameterBuilders));
            }
            sb.Append(")");
            sb.Append("\n{");
            sb.Append($"\n{codeBuilder.Build()}");
            sb.Append("\n}");
            return sb.ToString();
        }

        public static object BuildList(List<MethodBuilder> methodBuilders)
        {
            string result = string.Empty;
            for (int i = 0; i < methodBuilders.Count; i++)
            {
                result += $"{methodBuilders[i].Build()}\n";
            }
            return result;
        }
    }
}
