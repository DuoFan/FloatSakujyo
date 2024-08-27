using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace GameExtension
{
    public class AddressableManager : MonoBehaviour
    {
        public static AddressableManager Instance { get; private set; }
        [SerializeField] NativeAddressableConfig nativeAddressableConfig;
        public Dictionary<string, UnityEngine.Object> NativeAssets { get; private set; }
        public Dictionary<string, UnityEngine.Object[]> NativeAssetsList { get; private set; }
        Dictionary<string, Sprite> nativeSprites;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Init();
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
            }
        }
        private void Init()
        {
            if(nativeAddressableConfig == null)
            {
                return;
            }

            NativeAssets = new Dictionary<string, UnityEngine.Object>();
            for (int i = 0; i < nativeAddressableConfig.Assets.Length; i++)
            {
                NativeAssets.Add(nativeAddressableConfig.Assets[i].address, nativeAddressableConfig.Assets[i].asset);
            }
            NativeAssetsList = new Dictionary<string, UnityEngine.Object[]>();
            for (int i = 0; i < nativeAddressableConfig.AssetsLists.Length; i++)
            {
                var assets = nativeAddressableConfig.AssetsLists[i].assets;
                var objAssets = new UnityEngine.Object[assets.Length];
                for (int j = 0; j < objAssets.Length; j++)
                {
                    objAssets[j] = assets[j].asset;
                }
                NativeAssetsList.Add(nativeAddressableConfig.AssetsLists[i].address, objAssets);
            }
            nativeAddressableConfig = null;
            if (NativeAssets.Count == 0)
            {
                NativeAssets.TrimExcess();
            }
            if (NativeAssetsList.Count == 0)
            {
                NativeAssetsList.TrimExcess();
            }
        }
        public LoadAssetHandle<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
        {
            LoadAssetHandle<T> handle = new LoadAssetHandle<T>();
            handle.Initialize(address);
            return handle;
        }
        public LoadAssetsHandle<T> LoadAssetsAsync<T>(string address) where T : UnityEngine.Object
        {
            LoadAssetsHandle<T> handle = new LoadAssetsHandle<T>();
            handle.Initialize(address);
            return handle;
        }
        public LoadAssetHandle<T> LoadRemoteAssetAsync<T>(string address) where T : UnityEngine.Object
        {
            LoadAssetHandle<T> handle = new LoadAssetHandle<T>();
            handle.LoadRemoteAssetAsync(address);
            return handle;
        }
        public LoadAssetsHandle<T> LoadRemoteAssetsAsync<T>(string address) where T : UnityEngine.Object
        {
            LoadAssetsHandle<T> handle = new LoadAssetsHandle<T>();
            handle.LoadRemoteAssetsAsync(address);
            return handle;
        }
        public bool TryLoadLocalAsset<T>(string address, out T obj) where T : UnityEngine.Object
        {
            obj = null;

            if (NativeAssets == null)
            {
                return false;
            }

            if (typeof(T).Equals(typeof(Sprite)))
            {
                if (nativeSprites == null)
                {
                    nativeSprites = new Dictionary<string, Sprite>();
                }
                if(nativeSprites.TryGetValue(address,out Sprite sprite))
                {
                    obj = sprite as T;
                    return true;
                }
                else if (NativeAssets.TryGetValue(address, out UnityEngine.Object _obj))
                {
                    var texture2D = _obj as Texture2D;
                    if (!nativeSprites.ContainsKey(address))
                    {
                        sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.one * 0.5f);
                        nativeSprites.Add(address, sprite);
                    }
                    NativeAssets.Remove(address);
                    obj = sprite as T;
                    return true;
                }
                else
                {
                    if(NativeAssetsList.TryGetValue(address, out UnityEngine.Object[] _objs))
                    {
                        var error = $"尝试通过{address}加载资源,但是该地址对应的是资源列表,不能通过TryLoadLocalAsset加载单一资源";
                        GameExtension.Logger.Error(error);
                    }
                    return false;
                }
            }
            else if (NativeAssets.TryGetValue(address, out UnityEngine.Object _obj))
            {
                obj = _obj as T;
                return true;
            }
            else
            {
                if (NativeAssetsList != null && NativeAssetsList.TryGetValue(address, out UnityEngine.Object[] _objs))
                {
                    var error = $"尝试通过{address}加载资源,但是该地址对应的是资源列表,不能通过TryLoadLocalAsset加载单一资源";
                    GameExtension.Logger.Error(error);
                }
                return false;
            }
        }
        public bool TryLoadLocalAssets<T>(string address, out T[] objs) where T : UnityEngine.Object
        {
            objs = null;

            if (NativeAssetsList == null)
            {
                return false;
            }

            if (NativeAssetsList.TryGetValue(address, out UnityEngine.Object[] _objs))
            {
                objs = new T[_objs.Length];
                for (int i = 0; i < _objs.Length; i++)
                {

                    if (typeof(T).Equals(typeof(Sprite)))
                    {
                        var texture2D = _objs[i] as Texture2D;
                        var sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.one * 0.5f);
                        objs[i] = sprite as T;
                    }
                    else
                    {
                        objs[i] = _objs[i] as T;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public class LoadAssetHandle<T> where T : UnityEngine.Object
    {
        public T Result { get; private set; }
        AsyncOperationHandle<T> mHandle;
        public event Action<T> Completed
        {
            add
            {
                if (IsDone)
                {
                    if(Result == null)
                    {
                        Result = mHandle.Result;
                    }
                    value?.Invoke(Result);
                }
                else
                {
                    completed += value;
                }
            }
            remove
            {
                completed -= value;
            }
        }
        Action<T> completed;
        public bool IsDone => Result != null || mHandle.IsDone;
        string address;
        public void Initialize(string _address)
        {
            address = _address;
            if (AddressableManager.Instance.TryLoadLocalAsset(address, out T _Result))
            {
                Result = _Result;
            }
            else
            {
                LoadRemoteAssetAsync(address);
            }
        }
        public void LoadRemoteAssetAsync(string _address)
        {
            address = _address;
            mHandle = Addressables.LoadAssetAsync<T>(address);
            mHandle.Completed += OnLoadedAsset;
        }
        public IEnumerator WaitForCompletion()
        {
            yield return new WaitUntil(() => IsDone);
        }
        void OnLoadedAsset(AsyncOperationHandle<T> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Result = handle.Result;
                completed?.Invoke(Result);
            }
            else
            {
                GameExtension.Logger.Log($"加载资源失败:{address}:{handle.OperationException.Message}");
            }
        }
        public void Release()
        {
            completed = null;
            Result = null;
            if (mHandle.IsValid())
            {
                Addressables.Release(mHandle);
            }
        }
    }
    public class LoadAssetsHandle<T> where T : UnityEngine.Object
    {
        public IList<T> Result { get; private set; }
        AsyncOperationHandle<IList<T>> mHandle;
        public event Action<IList<T>> Completed;
        public bool IsDone => Result != null || mHandle.IsDone;
        string address;
        public void Initialize(string _address)
        {
            address = _address;
            if (AddressableManager.Instance.TryLoadLocalAssets(address, out T[] _Result))
            {
                Result = _Result;
                //等待完成事件注册完成后再触发
                IEnumerator WaitForCompleted()
                {
                    yield return null;
                    Completed?.Invoke(Result);
                }
                AddressableManager.Instance.StartCoroutine(WaitForCompleted());
            }
            else
            {
                LoadRemoteAssetsAsync(address);
            }
        }
        public void LoadRemoteAssetsAsync(string _address)
        {
            address = _address;
            mHandle = Addressables.LoadAssetsAsync<T>(address, null);
            mHandle.Completed += OnLoadedAsset;
        }
        public IEnumerator WaitForCompletion()
        {
            yield return new WaitUntil(() => IsDone);
        }
        void OnLoadedAsset(AsyncOperationHandle<IList<T>> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Result = handle.Result;
                Completed?.Invoke(Result);
                GameExtension.Logger.Log($"加载资源成功:{address}");
            }
            else
            {
                GameExtension.Logger.Log($"加载资源失败:{address}:{handle.OperationException.Message}");
            }
        }
        public void Release()
        {
            if (mHandle.IsValid())
            {
                Addressables.Release(mHandle);
            }
        }
    }
}
