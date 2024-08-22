using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GameExtension;

namespace EditorExtension
{
    public class AttributeBuilder : IBuild<string>
    {
        public string attributeType;
        public List<object> parameters;
        public void SetAttributeType(System.Type _attributeType)
        {
            attributeType = EditorUtils.GetTypeName(_attributeType);
        }
        public void AddParameter(object parameter)
        {
            if (parameters == null)
            {
                parameters = new List<object>();
            }
            parameters.Add(parameter);
        }
        public string Build()
        {
            if (attributeType == null)
            {
                throw new ParameterNullException("attributeType");
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.Append(attributeType);
            if (parameters != null)
            {
                sb.Append("(");
                for (int i = 0; i < parameters.Count; i++)
                {
                    sb.Append(ParseParmeter(parameters[i]));
                    if (i < parameters.Count - 1)
                    {
                        sb.Append(",");
                    }
                }
                sb.Append(")");
            }
            sb.Append("]");
            return sb.ToString();
        }

        static string ParseParmeter(object parmeter)
        {
            if (parmeter.GetType() == typeof(string))
            {
                return $"\"{parmeter}\"";
            }
            else if (parmeter.GetType() == typeof(char))
            {
                return $"\'{parmeter}\'";
            }
            else if (parmeter.GetType() == typeof(bool))
            {
                return $"{parmeter.ToString().ToLower()}";
            }
            else if (parmeter.GetType().IsEnum)
            {
                return $"{EditorUtils.GetTypeName(parmeter.GetType())}.{parmeter}";
            }
            else
            {
                return $"{parmeter}";
            }
        }
        public static string BuildList(List<AttributeBuilder> attributeBuilders)
        {
            string result = string.Empty;
            for (int i = 0; i < attributeBuilders.Count; i++)
            {
                result += $"{attributeBuilders[i].Build()}\n";
            }
            return result;
        }
    }
}
