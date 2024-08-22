using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameExtension
{
    public abstract class GameInitializerBase : MonoBehaviour, IGameInitializer
    {
        int tasks;
        protected Queue<IGameInitializer> gameInitializers;

        [SerializeField]
        protected GameObjectInstantiateTask[] gameObjectInstantiateTasks;

        [SerializeField]
        protected LoadSheetDataTask[] loadSheetDataTasks;


        protected virtual void Awake()
        {
            Application.targetFrameRate = 60;
            StartCoroutine(InitializeGame());
        }
        public abstract IEnumerator InitializeGame();

        protected IEnumerator LoadAllSheetData(float deltaProgress)
        {
            DataUtils.SortByPriority(loadSheetDataTasks);
            for (int i = 0; i < loadSheetDataTasks.Length; i++)
            {
                var task = loadSheetDataTasks[i];
                var type = Type.GetType(task.sheetDataManagerTypeName);
                var manager = Activator.CreateInstance(type);
                var sheetDataManager = manager as SheetDataManagerBase;
                LoadSheetData(task.assetReference, sheetDataManager, task.assetReference, deltaProgress);
            }
            yield return WaitForAllTaskCompleted();
        }

        protected IEnumerator InstantiateGameObjects()
        {
            UnityEngine.Object Instantiate(UnityEngine.Object prototype)
            {
                var obj = GameObject.Instantiate(prototype);
                LoadingPanel.Instance?.SetLoadingText($"加载{prototype.name}成功");
                return obj;
            }

            DataUtils.SortByPriority(gameObjectInstantiateTasks);

            for (int i = 0; i < gameObjectInstantiateTasks.Length; i++)
            {
                var obj = Instantiate(gameObjectInstantiateTasks[i].gameObject);
                var initializers = obj.GetComponents<IGameInitializer>();
                for (int j = 0; j < initializers.Length; j++)
                {
                    EnqueueInitializer(initializers[j]);
                }
            }

            yield break;
        }

        protected void LoadConfigData<T>(string assetReference, ConfigDataManagerBase<T> dataManager,
            string dataType, float deltaProgress) where T : IConfigData
        {
            /*tasks++;
            var handle = AddressableManager.Instance.LoadAssetAsync<TextAsset>(assetReference);
            handle.Completed += x =>
            {
                try
                {
                    JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    };
                    var configDatas = JsonConvert.DeserializeObject<T[]>(x.text, jsonSerializerSettings);
                    dataManager.Init(configDatas);
                    GameExtension.Logger.Log($"加载{dataType}配置成功");
                }
                catch (Exception)
                {
                    GameExtension.Logger.Log($"加载配置失败:{x.name}");
                }

                LoadingPanel.Instance.Progress += deltaProgress;
                handle.Release();
                tasks--;
            };*/
        }

        protected void LoadConfigDatas<T>(string levelsConfigReference, ConfigDataManagerBase<T> dataManager,
            string dataType, float deltaProgress) where T : IConfigData
        {
            /*tasks++;
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            var handle = AddressableManager.Instance.LoadAssetsAsync<TextAsset>(levelsConfigReference);
            handle.Completed += result =>
            {
                int i = 0;
                T[] datas = new T[result.Count];
                var _deltaProgress = deltaProgress / result.Count;
                StartCoroutine(InternalInit());

                IEnumerator InternalInit()
                {
                    foreach (var configAsset in result)
                    {
                        try
                        {
                            datas[i] = JsonConvert.DeserializeObject<T>(configAsset.text, jsonSerializerSettings);
                            i++;
                            LoadingPanel.Instance.Progress += _deltaProgress;
                        }
                        catch (Exception e)
                        {
                            GameExtension.Logger.Log($"加载{dataType}配置[{i}]失败:{e.Message}");
                        }

                        yield return null;
                    }

                    dataManager.Init(datas);
                    LoadingPanel.Instance.SetLoadingText($"加载{dataType}配置成功");
                    GameExtension.Logger.Log($"加载{dataType}配置成功");
                    handle.Release();
                    tasks--;
                }
            };*/
        }

        protected private void LoadSheetData(string assetReference, SheetDataManagerBase sheetDataManager,
            string dataType, float deltaProgress)
        {
            tasks++;
            var handle = AddressableManager.Instance.LoadAssetAsync<TextAsset>(assetReference);
            handle.Completed += (x) =>
            {
                var spreadsheet = new ES3Spreadsheet();
                spreadsheet.LoadRaw(x.text);
                StartCoroutine(InternalInit());

                IEnumerator InternalInit()
                {
                    yield return sheetDataManager.IEInit(spreadsheet);
                    GameExtension.Logger.Log($"加载{dataType}配置成功");
                    if(LoadingPanel.Instance != null)
                    {
                        LoadingPanel.Instance.SetLoadingText($"加载{dataType}配置成功");
                        LoadingPanel.Instance.Progress += deltaProgress;
                    }
                    handle.Release();
                    tasks--;
                }
            };
        }

        protected private void LoadSheetDatas<T>(string sheetReference, SheetDataManagerBase<T> sheetDataManager,
            string dataType, float deltaProgress) where T : IConfigData
        {
            tasks++;
            var handle = AddressableManager.Instance.LoadAssetsAsync<TextAsset>(sheetReference);
            handle.Completed += result =>
            {
                var spreadsheet = new ES3Spreadsheet();
                StartCoroutine(InternalInit());

                IEnumerator InternalInit()
                {
                    for (int i = 0; i < result.Count; i++)
                    {
                        spreadsheet.LoadRaw(result[i].text);
                        yield return sheetDataManager.IEInit(spreadsheet);
                    }

                    GameExtension.Logger.Log($"加载{dataType}配置成功");
                    LoadingPanel.Instance.SetLoadingText($"加载{dataType}配置成功");
                    LoadingPanel.Instance.Progress += deltaProgress;
                    handle.Release();
                    tasks--;
                }
            };
        }

        protected IEnumerator WaitForLoadSheetDatas<T>(string sheetReference, SheetDataManagerBase<T> sheetDataManager,
            string dataType, float deltaProgress) where T : IConfigData
        {
            var handle = AddressableManager.Instance.LoadAssetsAsync<TextAsset>(sheetReference);
            yield return handle.WaitForCompletion();
            var spreadsheet = new ES3Spreadsheet();
            for (int i = 0; i < handle.Result.Count; i++)
            {
                spreadsheet.LoadRaw(handle.Result[i].text);
                yield return sheetDataManager.IEInit(spreadsheet);
            }

            GameExtension.Logger.Log($"加载{dataType}配置成功");
            LoadingPanel.Instance.SetLoadingText($"加载{dataType}配置成功");
            LoadingPanel.Instance.Progress += deltaProgress;
            handle.Release();
        }

        protected IEnumerator WaitForAllTaskCompleted()
        {
            while (tasks > 0)
            {
                yield return null;
            }
        }

        protected void EnqueueInitializer(IGameInitializer obj)
        {
            if(gameInitializers == null)
            {
                gameInitializers = new Queue<IGameInitializer>();
            }
            gameInitializers.Enqueue(obj);
        }

        protected IEnumerator ExecuteAllInitializer()
        {
            if(gameInitializers == null)
            {
                yield break;
            }
            while(gameInitializers.Count > 0)
            {
                yield return gameInitializers.Dequeue().InitializeGame();
            }
        }

        [System.Serializable]
        protected struct GameObjectInstantiateTask:IPrioirtyData
        {
            public GameObject gameObject;

            public int Priority => priority;
            [SerializeField]
            int priority;
        }

        [System.Serializable]
        protected struct LoadSheetDataTask : IPrioirtyData
        {
            public string assetReference;
            public string sheetDataManagerTypeName;
            public int Priority => priority;
            [SerializeField]
            int priority;
        }

        [System.Serializable]
        protected struct LoadMultiConfigDataTask : IPrioirtyData
        {
            public string configReference;
            public string configDataManagerTypeName;
            public int Priority => priority;
            [SerializeField]
            int priority;
        }
    }

    public interface IGameInitializer
    {
        IEnumerator InitializeGame();
    }
}