using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace GameExtension
{
    [System.Serializable]
    public class SerializedParameters : ICloneable
    {
        Dictionary<string, object> runtimeObjectValueDict;
        object[] runtimeObjectValueArray;
        [HideInInspector] public SerializedParameter[] parameters;
        [JsonIgnore] public bool IsEmpty { get; private set; }

        [JsonConstructor]
        public SerializedParameters(params SerializedParameter[] parameters)
        {
            if (SerializeUtils.IsEditorMode)
            {
                InitInEditor(parameters);
            }
            else
            {
                InitInRuntime(parameters);
            }
        }

        /// <summary>
        /// 仅允许编辑器调用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SetParameter(string key, object value)
        {
            if (!SerializeUtils.IsEditorMode)
            {
                throw new InvalidOperationException("仅允许在编辑器模式下调用该方法");
            }

            if (parameters == null)
            {
                parameters = new SerializedParameter[0];
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].key.Equals(key))
                {
                    parameters[i].value = SerializeUtils.Serialize(value);
                    return;
                }
            }

            var newParameters = new SerializedParameter[parameters.Length + 1];
            Array.Copy(parameters, newParameters, parameters.Length);
            newParameters[parameters.Length] = new SerializedParameter(key, SerializeUtils.Serialize(value));
            parameters = newParameters;
        }
        public bool TryGetParameter<T>(string key, out object value)
        {
            value = GetParameter(key, typeof(T));
            return value != null;
        }

        public T GetParameter<T>(string key)
        {
            var value = GetParameter(key, typeof(T));
            if (value != null)
            {
                return (T)value;
            }
            else
            {
                return default;
            }
        }

        public object GetParameter(string key, Type type)
        {
            object objectValue;
            if (SerializeUtils.IsEditorMode)
            {
                objectValue = GetObjectValueInEditor(key, type);
            }
            else
            {
                objectValue = IsEmpty ? null : GetObjectValueInRuntime(key, type);
            }

            return objectValue;
        }

        public string GetObjectAddress(string key)
        {
            return GetParameter<string>(key);
        }

        public SerializedParametersContainer GetParametersContainer(string key)
        {
            return GetParameter<SerializedParametersContainer>(key);
        }

        void InitInEditor(SerializedParameter[] parameters)
        {
            this.parameters = parameters;
            IsEmpty = parameters == null || parameters.Length == 0;
        }
        void InitInRuntime(SerializedParameter[] parameters)
        {
            IsEmpty = parameters == null || parameters.Length == 0;

            // 根据参数数量选择是使用字典还是数组
            if (!IsEmpty)
            {
                this.parameters = parameters;
                if (parameters.Length > 10)
                {
                    runtimeObjectValueDict = new Dictionary<string, object>(parameters.Length);
                }
                else
                {
                    runtimeObjectValueArray = new object[parameters.Length];
                }
            }
        }

        object GetObjectValueInEditor(string key, Type type)
        {
            if (parameters == null) return null;
            string stringValue = null;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].key.Equals(key))
                {
                    stringValue = parameters[i].value;
                    break;
                }
            }

            return SerializeUtils.Deserialize(stringValue, type);
        }


        private object GetObjectValueInRuntime(string key, Type type)
        {
            if (runtimeObjectValueArray != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].key.Equals(key))
                    {
                        if (runtimeObjectValueArray[i] == null)
                        {
                            runtimeObjectValueArray[i] = SerializeUtils.Deserialize(parameters[i].value, type);
                        }
                        return runtimeObjectValueArray[i];
                    }
                }
            }
            else if (runtimeObjectValueDict != null)
            {
                if (!runtimeObjectValueDict.TryGetValue(key, out object objectValue))
                {
                    string stringValue = null;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].key.Equals(key))
                        {
                            stringValue = parameters[i].value;
                        }
                    }
                    objectValue = SerializeUtils.Deserialize(stringValue, type);
                    if (objectValue != null)
                    {
                        runtimeObjectValueDict[key] = objectValue;
                        //所有参数都已经被初始化
                        if(runtimeObjectValueDict.Count == parameters.Length)
                        {
                            parameters = null;
                        }
                    }
                }
                return objectValue;
            }

            return null;
        }

        public object Clone()
        {
            SerializedParameter[] serializedParameters = new SerializedParameter[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                serializedParameters[i] = new SerializedParameter(parameters[i].key, parameters[i].value);
            }
            return new SerializedParameters(serializedParameters);
        }
    }

    [System.Serializable]
    public class SerializedParameter
    {
        public string key;
        public string value;

        [JsonConstructor]
        public SerializedParameter(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }

}