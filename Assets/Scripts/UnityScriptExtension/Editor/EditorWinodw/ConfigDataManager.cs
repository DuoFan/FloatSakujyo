using GameExtension;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EditorExtension
{
    public class ConfigDataManager<T> : ConfigDataManager<T>.IConfigDataManageStrategy where T : IConfigData
    {
        IConfigDataManageStrategy manageStrategy;
        public ConfigDataManager(ConfigDataManagerInitContext initContext)
        {
            Init(initContext);
        }

        public void Init(ConfigDataManagerInitContext initContext)
        {
            switch (initContext.manageStrategy)
            {
                case ConfigDataManageStrategy.FromFile:
                    this.manageStrategy = new FileConfigDataManageStrategy();
                    break;
                case ConfigDataManageStrategy.FromDirectory:
                    this.manageStrategy = new DirectoryConfigDataManageStrategy();
                    break;
            }
            manageStrategy.Init(initContext);
        }

        public int GetNewDataID()
        {
            return manageStrategy.GetNewDataID();
        }
        public T[] GetDatas()
        {
            return manageStrategy.GetDatas();
        }
        public T GetDataByIndex(int index)
        {
            return manageStrategy.GetDataByIndex(index);
        }
        public T GetDataByID(int id)
        {
            return manageStrategy.GetDataByID(id);
        }

        public void DeleteData(int id)
        {
            manageStrategy.DeleteData(id);
        }
        public void SaveData(T data)
        {
            manageStrategy.SaveData(data);
        }
        interface IConfigDataManageStrategy
        {
            void Init(ConfigDataManagerInitContext initContext);
            int GetNewDataID();

            T[] GetDatas();

            T GetDataByID(int id);

            T GetDataByIndex(int index);

            void DeleteData(int id);
            void SaveData(T data);
        }
        class FileConfigDataManageStrategy : IConfigDataManageStrategy
        {
            ConfigDataManagerInitContext initContext;
            SortedList<int, T> datas;
            public void Init(ConfigDataManagerInitContext _initContext)
            {
                initContext = _initContext;
                datas = new SortedList<int, T>();
                if (File.Exists(initContext.configPath))
                {
                    var json = File.ReadAllText(initContext.configPath);
                    JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
                    jsonSerializerSettings.TypeNameHandling = initContext.typeNameHandling;
                    var _datas = JsonConvert.DeserializeObject<T[]>(json, jsonSerializerSettings);
                    for (int i = 0; i < _datas.Length; i++)
                    {
                        var data = _datas[i];
                        datas[data.ID] = data;
                    }
                }
            }

            public int GetNewDataID()
            {
                if (datas.Count == 0)
                {
                    return 0;
                }
                else
                {
                    return datas.Values[datas.Values.Count - 1].ID + 1;
                }
            }

            public T[] GetDatas()
            {
                return datas.Values.ToArray() ?? null;
            }

            public T GetDataByID(int id)
            {
                return datas[id];
            }

            public T GetDataByIndex(int index)
            {
                var id = datas.Keys[index];
                return datas[id];
            }

            public void DeleteData(int id)
            {
                datas.Remove(id);
            }
            public void SaveData(T data)
            {
                datas[data.ID] = data;
                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
                jsonSerializerSettings.TypeNameHandling = initContext.typeNameHandling;
                var json = JsonConvert.SerializeObject(GetDatas(), jsonSerializerSettings);
                File.WriteAllText(initContext.configPath, json);
                Debug.Log($"保存了配置文件:{initContext.configPath}");

                AssetDatabase.Refresh();
            }
        }
        class DirectoryConfigDataManageStrategy : IConfigDataManageStrategy
        {
            ConfigDataManagerInitContext initContext;
            SortedList<int, T> datas;
            Queue<T> waitToDeleteDatas;
            public void Init(ConfigDataManagerInitContext _initContext)
            {
                initContext = _initContext;
                datas = new SortedList<int, T>();
                waitToDeleteDatas = new Queue<T>();
                if (Directory.Exists(initContext.configPath))
                {
                    JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
                    jsonSerializerSettings.TypeNameHandling = initContext.typeNameHandling;
                    var dataFiles = EditorUtils.FindFiles(initContext.configPath,
                        new System.Text.RegularExpressions.Regex($"{initContext.dataPrefix}[0-9]+\\.json$"));
                    for (int i = 0; i < dataFiles.Length; i++)
                    {
                        var data = JsonConvert.DeserializeObject<T>(File.ReadAllText(dataFiles[i]),
                            jsonSerializerSettings);
                        datas[data.ID] = data;
                    }
                }
            }

            public int GetNewDataID()
            {
                if (datas.Count == 0)
                {
                    return 0;
                }
                else
                {
                    return datas.Values[datas.Values.Count - 1].ID + 1;
                }
            }

            public T[] GetDatas()
            {
                return datas.Values.ToArray() ?? null;
            }

            public T GetDataByID(int id)
            {
                return datas[id];
            }

            public T GetDataByIndex(int index)
            {
                var id = datas.Keys[index];
                return datas[id];
            }

            public void DeleteData(int id)
            {
                waitToDeleteDatas.Enqueue(GetDataByID(id));
                datas.Remove(id);
            }
            public void SaveData(T data)
            {
                datas[data.ID] = data;
                var dataPath = Path.Combine(initContext.configPath, $"{initContext.dataPrefix}{data.ID}.json");
                var dir = Path.GetDirectoryName(dataPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
                jsonSerializerSettings.TypeNameHandling = initContext.typeNameHandling;
                string json = JsonConvert.SerializeObject(data, jsonSerializerSettings);
                File.WriteAllText(dataPath, json);

                HandleDeleteQueue();

                AssetDatabase.Refresh();

                var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(dataPath.Replace(Application.dataPath,
                    "Assets"));
                var entry = EditorUtils.GetAddressableAssetEntry(textAsset) ??
                    EditorUtils.CreateEntryToGroupByGroupName(textAsset, initContext.dataAddressableGroup);
                entry.address = initContext.dataAddress;
            }
            void HandleDeleteQueue()
            {
                while (waitToDeleteDatas.Count > 0)
                {
                    var data = waitToDeleteDatas.Dequeue();
                    var dataPath = Path.Combine(initContext.configPath, $"{initContext.dataPrefix}{data.ID}.json");
                    if (File.Exists(dataPath))
                    {
                        var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(dataPath.Replace(Application.dataPath,
                            "Assets"));
                        EditorUtils.RemoveAddressableAssetEntry(textAsset);
                        File.Delete(dataPath);
                    }
                }
            }
        }
    }
    public enum ConfigDataManageStrategy
    {
        FromFile, FromDirectory
    }

    public class ConfigDataManagerInitContext
    {
        public ConfigDataManageStrategy manageStrategy;
        public string configPath;
        public string dataPrefix;
        public string dataAddress;
        public string dataAddressableGroup;
        public TypeNameHandling typeNameHandling = TypeNameHandling.Auto;
    }
}

