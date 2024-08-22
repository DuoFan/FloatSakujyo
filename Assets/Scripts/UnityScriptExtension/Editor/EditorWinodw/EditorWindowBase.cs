using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Codice.CM.Common.Selectors;
using UnityEditor.PackageManager.UI;
using GameExtension;

namespace EditorExtension
{
    public interface IEditorWindowBase
    {
        public event Action OnSaveNewData;
        public event Action OnEditData;
        public event Action OnDeleteData;
        public ConfigDataManager<T> GetDataManager<T>() where T : IConfigData;
        public void SaveNewData<T>(T data) where T : IConfigData;
        public void DeleteData<T>(int id) where T : IConfigData;
        public void EditData<T>(T data) where T : IConfigData;
        public T GetDataByID<T>(int id) where T : IConfigData;
        public T GetDataByIndex<T>(int index) where T : IConfigData;
        public int GetDataIndexByID<T>(int id) where T : IConfigData;
        public T[] GetDatas<T>() where T : IConfigData;
        public int GetNewDataID<T>() where T : IConfigData;
        public ObjectSelector<T> ProvideDataSelector<T>() where T : IConfigData;
        public void LoadAddressableAsset<T>(ref T assetRef, string assetAddress) where T : UnityEngine.Object;
        public void LoadTextureAsSprite(out Sprite sprite, string textureAddress);
        public string GetAssetAddress<T>(T asset) where T : UnityEngine.Object;
        void AddMessage(string v1, string v2, MessageType error);
        void DismissMessage(string v);
    }
    public abstract partial class EditorWindowBase<TData> : Editor<TData>, IEditorWindowBase
        where TData : IConfigData
    {
        public event Action OnSaveNewData;
        public event Action OnEditData;
        public event Action OnDeleteData;

        protected Dictionary<Type, object> dataManagers;
        protected Dictionary<Type, object> sheetManagers;

        Dictionary<Type, ObjectOptionProvider> objectOptionProviders = new Dictionary<Type, ObjectOptionProvider>();
        public ConfigDataManager<T> GetDataManager<T>() where T : IConfigData
        {
            Type dataType = typeof(T);
            if (dataManagers.TryGetValue(dataType, out var manager))
            {
                return manager as ConfigDataManager<T>;
            }
            return null;
        }
        public SheetDataManagerBase<T> GetSheetDataManager<T>() where T : IConfigData
        {
            Type dataType = typeof(T);
            if (sheetManagers.TryGetValue(dataType, out var manager))
            {
                return manager as SheetDataManagerBase<T>;
            }
            return null;
        }
        public void SaveNewData<T>(T data) where T : IConfigData
        {
            GetDataManager<T>().SaveData(data);
            OnSaveNewData?.Invoke();
        }
        public void DeleteData<T>(int id) where T : IConfigData
        {
            GetDataManager<T>().DeleteData(id);
            OnDeleteData?.Invoke();
        }
        public void EditData<T>(T data) where T : IConfigData
        {
            GetDataManager<T>().SaveData(data);
            OnEditData?.Invoke();
        }
        public T GetDataByID<T>(int id) where T : IConfigData
        {
            var manager = GetDataManager<T>();

            if (manager != null)
            {
                return manager.GetDataByID(id);
            }

            var sheetManager = GetSheetDataManager<T>();
            if (sheetManager != null)
            {
                return sheetManager.GetData(id);
            }

            return default;
        }
        public T GetDataByIndex<T>(int index) where T : IConfigData
        {
            return GetDataManager<T>().GetDataByIndex(index);
        }
        public int GetDataIndexByID<T>(int id) where T : IConfigData
        {
            int index = -1;
            var datas = GetDatas<T>();
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i].ID == id)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        public T[] GetDatas<T>() where T : IConfigData
        {
            var manager = GetDataManager<T>();
            if (manager != null)
            {
                return manager.GetDatas();
            }
            var sheetManager = GetSheetDataManager<T>();
            if (sheetManager != null)
            {
                return sheetManager.GetDatas();
            }
            return null;
        }
        public int GetNewDataID<T>() where T : IConfigData
        {
            return GetDataManager<T>().GetNewDataID();
        }
        public ObjectSelector<T> ProvideDataSelector<T>() where T : IConfigData
        {
            var dataSelector = new ObjectSelector<T>();

            if (!objectOptionProviders.TryGetValue(typeof(T), out var optionProvider))
            {
                optionProvider = new ObjectOptionProvider();
                var datas = GetDatas<T>();
                for (int i = 0; i < datas.Length; i++)
                {
                    optionProvider.AddOption($"[{datas[i].ID}]:{datas[i].DataName}", datas[i]);
                }
                objectOptionProviders[typeof(T)] = optionProvider;
            }

            dataSelector.SetOptionProvider(optionProvider);
            dataSelector.SetOptions(optionProvider.Options, optionProvider.OptionMap);
            return dataSelector;
        }
        public void LoadAddressableAsset<T>(ref T assetRef, string assetAddress) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(assetAddress))
            {
                assetRef = (default);
                return;
            }

            assetRef = EditorUtils.LoadAddressableAsset<T>(assetAddress);
            if (assetRef == null)
            {
                Debug.LogError($"加载资源:{assetAddress}失败");
            }
        }
        public void LoadTextureAsSprite(out Sprite sprite, string textureAddress)
        {
            sprite = null;

            if (string.IsNullOrEmpty(textureAddress))
            {
                return;
            }

            try
            {
                Texture2D texture2D = null;
                LoadAddressableAsset(ref texture2D, textureAddress);
                var assetPath = AssetDatabase.GetAssetPath(texture2D);
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载贴图{textureAddress}失败:{e.Message}");
            }
        }
        public string GetAssetAddress<T>(T asset) where T : UnityEngine.Object
        {
            string assetAddress = string.Empty;

            if (asset != null)
            {
                assetAddress = EditorUtils.GetAddressableAssetAddress(asset);
                if (string.IsNullOrEmpty(assetAddress))
                {
                    Debug.LogError($"{asset.name}未设置Address");
                }
            }
            return assetAddress;
        }
        protected partial Dictionary<Type, ConfigDataManagerInitContext> ProvideFileConfigPathes();
        protected partial Dictionary<Type, ConfigDataManagerInitContext> ProvideDirectoryConfigPathes();
        protected partial Dictionary<Type, SheetDataManagerInitContext> ProvideSheetDataPathes();
        protected virtual void InitConfigData(Dictionary<Type, ConfigDataManagerInitContext> configPaths)
        {
            if(configPaths == null)
            {
                return;
            }
            foreach (var entry in configPaths)
            {
                var dataManagerType = typeof(ConfigDataManager<>).MakeGenericType(entry.Key);
                var dataManagerInstance = Activator.CreateInstance(dataManagerType, entry.Value);
                dataManagers[entry.Key] = dataManagerInstance;
            }
        }
        void InitSheetData(Dictionary<Type, SheetDataManagerInitContext> sheetDataPathes)
        {
            if(sheetDataPathes == null)
            {
                return;
            }
            foreach (var item in sheetDataPathes)
            {
                var initContext = item.Value;
                var dataManagerType = initContext.managerType;
                var dataManagerInstance = Activator.CreateInstance(dataManagerType);
                var method = dataManagerType.GetMethod("Init", new Type[] { typeof(ES3Spreadsheet) });
                var data = File.ReadAllText(initContext.sheetPath);
                ES3Spreadsheet spreadsheet = new ES3Spreadsheet();
                spreadsheet.LoadRaw(data);
                method.Invoke(dataManagerInstance, new object[] { spreadsheet });
                sheetManagers[item.Key] = dataManagerInstance;
            }
        }
        protected EditorView<TData> GetCreateView<TCreateView, TEditorView>()
            where TCreateView : CreateViewBase<TData, TEditorView>, new()
            where TEditorView : EditorViewBase<TData>, new()
        {
            var editorView = GetEditorView<TEditorView>();
            var createView = new TCreateView();
            createView.SetBaseView(editorView);
            createView.SetBaseWindow(this);
            createView.Init();
            return createView;
        }
        protected EditorView<TData> GetEditView<TEditView, TEditorView>()
            where TEditView : EditViewBase<TData, TEditorView>, new()
            where TEditorView : EditorViewBase<TData>, new()
        {
            var editorView = GetEditorView<TEditorView>();
            var editView = new TEditView();
            editView.SetBaseView(editorView);
            editView.SetBaseWindow(this);
            editView.Init();
            return editView;
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            dataManagers = new Dictionary<Type, object>();
            InitConfigData(ProvideFileConfigPathes());
            InitConfigData(ProvideDirectoryConfigPathes());

            sheetManagers = new Dictionary<Type, object>();
            InitSheetData(ProvideSheetDataPathes());
        }

        T GetEditorView<T>() where T : EditorViewBase<TData>, new()
        {
            var view = new T();
            view.SetBaseWindow(this);
            view.Init();
            return view;
        }
    }
}
