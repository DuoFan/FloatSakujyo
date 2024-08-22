using GameExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class TypeInstanceCollection<T>
    {
        protected Dictionary<string, T> typeDict;
        public TypeInstanceCollection()
        {
            typeDict = new Dictionary<string, T>();
            var classSet = ClassSet.GetSet<T>();
            for (int i = 0; i < classSet.Item1.Length; i++)
            {
                typeDict[classSet.Item1[i]] = (T)Activator.CreateInstance(classSet.Item2[i]);
            }
        }

        public T GetTypeInstance(string typeName)
        {
            return typeDict[typeName];
        }
    }
}

