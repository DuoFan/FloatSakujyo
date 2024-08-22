using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GameExtension;


namespace EditorExtension
{
    public class ParameterBuilder : IBuild<string>
    {
        public string parameterType;
        public string parameterName;
        public ParameterModifier parameterModifier;
        public void SetParameterType(System.Type _parameterType)
        {
            parameterType = EditorUtils.GetTypeName(_parameterType);
        }
        public void SetParameterName(string _parameterName)
        {
            parameterName = _parameterName;
        }
        public void SetParameterModifier(ParameterModifier _parameterModifier)
        {
            parameterModifier = _parameterModifier;
        }
        public string Build()
        {
            if (parameterType == null)
            {
                throw new ParameterNullOrEmptyException("parameterType");
            }
            else if (string.IsNullOrEmpty(parameterName))
            {
                throw new ParameterNullOrEmptyException("parameterName");
            }
            var sb = new StringBuilder();
            if (parameterModifier != ParameterModifier.None)
            {
                sb.Append(parameterModifier.ToString().ToLower());
                sb.Append(" ");
            }
            sb.Append(parameterType);
            sb.Append(" ");
            sb.Append(parameterName);
            return sb.ToString();
        }
        public static string BuildeList(List<ParameterBuilder> parameterBuilders)
        {
            string result = string.Empty;
            for (int i = 0; i < parameterBuilders.Count; i++)
            {
                result += parameterBuilders[i].Build();
                if (i < parameterBuilders.Count - 1)
                {
                    result += ",";
                }
            }
            return result;
        }
    }

    public enum ParameterModifier
    {
        None, In, Out, Params
    }

}