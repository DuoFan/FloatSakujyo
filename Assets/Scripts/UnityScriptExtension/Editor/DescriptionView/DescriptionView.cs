using GameExtension;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace EditorExtension
{
    public class DescriptionView : IDraw
    {
        public List<IDraw> Elements
        {
            get; private set;
        }

        public void Draw()
        {
            if (Elements == null)
            {
                return;
            }
            for (int i = 0; i < Elements.Count; i++)
            {
                Elements[i].Draw();
            }
        }

        public void Set(object obj, bool isForceDescription = false)
        {
            var type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Elements = new List<IDraw>();
            DescriptionProviderAttribute descriptionProvider;
            for (int i = 0; i < properties.Length; i++)
            {
                descriptionProvider = properties[i].GetCustomAttribute<DescriptionProviderAttribute>();
                if (descriptionProvider != null || isForceDescription)
                {
                    Elements.Add(DescriptionElementFactory.GetElement(obj, properties[i], descriptionProvider?.description ?? properties[i].Name));
                }
            }
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; i++)
            {
                descriptionProvider = fields[i].GetCustomAttribute<DescriptionProviderAttribute>();
                if (descriptionProvider != null || isForceDescription)
                {
                    Elements.Add(DescriptionElementFactory.GetElement(obj, fields[i], descriptionProvider?.description ?? fields[i].Name));
                }
            }
        }
    }
}
