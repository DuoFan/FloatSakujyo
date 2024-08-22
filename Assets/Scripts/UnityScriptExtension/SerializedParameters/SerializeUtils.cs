using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace GameExtension
{
    public partial class SerializeUtils
    {
        static SerializeUtils Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SerializeUtils();
                    instance.Init();
                }
                return instance;
            }
        }
        static SerializeUtils instance;
        public static bool IsEditorMode { get; }
        bool isEditorMode;

        Dictionary<Type, Serializer> typeSerializer = new Dictionary<Type, Serializer>()
        {
            {
                typeof(int),new Serializer()
                {
                    Serialize = (objectValue) => objectValue.ToString(),
                    Deserialize = (stringValue) => int.Parse(stringValue)
                }
            },
            {
                typeof(float),new Serializer()
                {
                    Serialize = (objectValue) => objectValue.ToString(),
                    Deserialize = (stringValue) => float.Parse(stringValue)
                }
            },
            {
                typeof(bool),new Serializer()
                {
                    Serialize = (objectValue) => objectValue.ToString(),
                    Deserialize = (stringValue) => bool.Parse(stringValue)
                }
            },
            {
                typeof(string),new Serializer()
                {
                    Serialize = (objectValue) => objectValue.ToString(),
                    Deserialize = (stringValue) => stringValue.TrimEnd('\r')
                }
            },
            {
                typeof(Vector2),new Serializer()
                {
                    Serialize = (objectValue) => $"({((Vector2)objectValue).x},{((Vector2)objectValue).y})",
                    Deserialize = (stringValue) =>
                    {
                        string[] values = stringValue.Trim(new char[] { '(', ')' }).Split(',');
                        return new Vector2(float.Parse(values[0]), float.Parse(values[1]));
                    }
                }
            },
            {
                typeof(Vector2Int),new Serializer()
                {
                    Serialize = (objectValue) => $"({((Vector2Int)objectValue).x},{((Vector2Int)objectValue).y})",
                    Deserialize = (stringValue) =>
                    {
                        string[] values = stringValue.Trim(new char[] { '(', ')' }).Split(',');
                        return new Vector2Int(int.Parse(values[0]), int.Parse(values[1]));
                    }
                }
            },
            {
                typeof(Area),new Serializer()
                {
                    Serialize = (objectValue) =>
                    {
                        var areaPoint = (Area)objectValue;
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.Append($"({areaPoint.areaId})");
                        if(areaPoint.points == null || areaPoint.points.Length == 0)
                        {
                            return stringBuilder.ToString();
                        }
                        stringBuilder.Append(";");
                        var ve2Serializer = GetSerializerForType(typeof(Vector2));
                        for (int i = 0; i < areaPoint.points.Length; i++)
                        {
                            stringBuilder.Append(ve2Serializer.Serialize(areaPoint.points[i]));
                            if(i < areaPoint.points.Length - 1)
                            {
                                stringBuilder.Append(":");
                            }
                        }
                        return stringBuilder.ToString();
                    },
                    Deserialize = (stringValue) =>
                    {
                        var areaPoint = new Area();
                        var ve2Serializer = GetSerializerForType(typeof(Vector2));
                        var values = stringValue.Split(';');
                        areaPoint.areaId = int.Parse(values[0].Trim(new char[] { '(', ')' }));
                        if(values.Length > 1)
                        {
                            var ve2Values = values[1].Split(':');
                            areaPoint.points = new Vector2[ve2Values.Length];
                            for (int i = 0; i < ve2Values.Length; i++)
                            {
                                areaPoint.points[i] = (Vector2)ve2Serializer.Deserialize(ve2Values[i]);
                            }
                        }
                        return areaPoint;
                    }
                }
            },
            {
                typeof(ExtensionItemAmountInfo),new Serializer()
                {
                    Serialize = (objectValue) =>
                    {
                        var info = (ExtensionItemAmountInfo)objectValue;
                        return $"{info.ItemType}#{info.ID}#{info.Amount}";
                    },
                    Deserialize = (stringValue) =>
                    {
                        string[] info = stringValue.Split('#');
                        int itemType = int.Parse(info[0]);
                        int itemID = int.Parse(info[1]);
                        int amount = int.Parse(info[2]);
                        return new ExtensionItemAmountInfo(itemType,itemID, amount);
                    }
                }
            }
        };
        Dictionary<Type, Serializer> baseTypeSerializer = new Dictionary<Type, Serializer>()
        {
            {
                typeof(Enum),new Serializer()
                {
                    Serialize = (objectValue) => ((int)objectValue).ToString(),
                    Deserialize = (stringValue) => int.Parse(stringValue)
                }
            },
            {
                typeof(UnityEngine.Object),new Serializer()
                {
                    Serialize = (objectValue) => throw new InvalidOperationException($"不支持的参数序列化类型:{nameof(UnityEngine.Object)}"),
                    //此时返回的是该物体的Addressable路径
                    Deserialize = (stringValue) => stringValue
                }
            },
            {
                typeof(SerializedParametersContainer),new Serializer()
                {
                    Serialize = (objectValue) => JsonConvert.SerializeObject(objectValue),
                    Deserialize = (stringValue) =>
                    {
                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            return JsonConvert.DeserializeObject<SerializedParametersContainer>(stringValue);
                        }
                        return null;
                    }
                }
            }
        };

        partial void Init();

        void RegisterSerializer(Type type, Serializer serializer)
        {
            typeSerializer[type] = serializer;
        }

        public static void EnableEditorMode()
        {
            Instance.isEditorMode = true;
        }

        public static string Serialize(object objectValue)
        {
            if (objectValue == null)
            {
                return string.Empty;
            }

            // 检查对象是否为数组
            if (objectValue.GetType().IsArray)
            {
                var array = objectValue as Array;
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("[");
                // 遍历数组的每个元素
                for (int i = 0; i < array.Length; i++)
                {
                    stringBuilder.Append(Serialize(array.GetValue(i)));
                    if (i < array.Length - 1)
                    {
                        stringBuilder.Append("|");
                    }
                }
                stringBuilder.Append("]");
                return stringBuilder.ToString();
            }
            else
            {
                // 处理单个对象
                Serializer serializer = GetSerializerForType(objectValue.GetType());
                if (serializer != null)
                {
                    return serializer.Serialize(objectValue);
                }
                else
                {
                    var error = $"不支持的参数序列化类型:{objectValue.GetType()}";
                    GameExtension.Logger.Error(error);
                    throw new InvalidOperationException(error);
                }
            }
        }

        public static object Deserialize(string stringValue, Type type)
        {
            if (string.IsNullOrEmpty(stringValue))
            {
                return default;
            }

            // 检查类型是否为数组
            if (type.IsArray)
            {
                List<object> elements = new List<object>(10);
                var elementType = type.GetElementType();
                Serializer elementTypeSerializer = elementType.IsArray ? null : GetSerializerForType(elementType);
                int i = 0;

                // 去掉数组的头尾括号
                if (stringValue[0] == '[')
                {
                    stringValue = stringValue.Substring(1, stringValue.Length - 2);
                }

                while (i < stringValue.Length)
                {
                    if (stringValue[i] == '[')
                    {
                        // 查找与当前 '[' 对应的 ']'
                        int bracketCounter = 1;
                        int start = i + 1;
                        while (i < stringValue.Length - 1 && bracketCounter > 0)
                        {
                            i++;
                            if (stringValue[i] == '[')
                            {
                                bracketCounter++;
                            }
                            else if (stringValue[i] == ']') bracketCounter--;
                        }
                        string elementValue = stringValue.Substring(start, i - start);
                        elements.Add(Deserialize(elementValue, elementType));
                    }
                    else if (stringValue[i] == '|')
                    {
                        // 忽略，仅用作分隔符
                    }
                    else
                    {
                        // 对于没有括号的单层数组情况
                        int start = i;
                        while (i < stringValue.Length && stringValue[i] != '|')
                        {
                            i++;
                        }
                        string elementValue = stringValue.Substring(start, i - start);
                        elements.Add(elementTypeSerializer.Deserialize(elementValue));
                        continue;
                    }
                    i++;
                }

                Array array = Array.CreateInstance(elementType, elements.Count);
                for (int j = 0; j < elements.Count; j++)
                {
                    array.SetValue(elements[j], j);
                }
                return array;
            }
            else
            {
                // 处理单个对象
                Serializer serializer = GetSerializerForType(type);
                if (serializer != null)
                {
                    return serializer.Deserialize(stringValue);
                }
                else
                {
                    var error = $"不支持的参数序列化类型:{type}";
                    GameExtension.Logger.Error(error);
                    throw new InvalidOperationException(error);
                }
            }
        }

        static Serializer GetSerializerForType(Type type)
        {
            if (!Instance.typeSerializer.TryGetValue(type, out Serializer serialize))
            {
                foreach (var baseType in Instance.baseTypeSerializer.Keys)
                {
                    if (baseType.IsAssignableFrom(type))
                    {
                        serialize = Instance.baseTypeSerializer[baseType];
                        break;
                    }
                }
            }
            return serialize;
        }
    }

    public class Serializer
    {
        public Func<object, string> Serialize;
        public Func<string, object> Deserialize;
    }
}
