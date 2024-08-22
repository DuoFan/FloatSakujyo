using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EditorExtension
{
    public class AttributerBuilderManager :IBuild<string>
    {
        public List<AttributeBuilder> attributeBuilders;
        public void AddAttributer(System.Type attributeType, params object[] parameters)
        {
            if (attributeBuilders == null)
            {
                attributeBuilders = new List<AttributeBuilder>();
            }
            AttributeBuilder attributeBuilder = new AttributeBuilder();
            attributeBuilder.SetAttributeType(attributeType);
            if(parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    attributeBuilder.AddParameter(parameters[i]);
                }
            }
            attributeBuilders.Add(attributeBuilder);
        }

        public string Build()
        {
            return AttributeBuilder.BuildList(attributeBuilders);
        }
    }
}
