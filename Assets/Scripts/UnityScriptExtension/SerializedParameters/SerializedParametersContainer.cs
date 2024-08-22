using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    [Serializable]
    public class SerializedParametersContainer
    {
        /// <summary>
        /// 将该参数传递给的类型名称
        /// </summary>
        public string TypeName
        {
            get; private set;
        }
        public SerializedParameters SerializedParameters
        {
            get; private set;
        }

        [JsonConstructor]
        public SerializedParametersContainer(string typeName, SerializedParameters serializedParameters)
        {
            TypeName = typeName;
            SerializedParameters = serializedParameters;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

