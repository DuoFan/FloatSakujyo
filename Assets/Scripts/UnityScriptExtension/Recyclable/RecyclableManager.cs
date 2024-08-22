using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameExtension
{
    public class RecyclableManager : SingletonMonoBase<RecyclableManager>
    {
        const float DEFAULT_TIMEOUT = 30;
        Dictionary<string, Pool<IRecyclable>> pools;

        public bool HasObjectPool(string address)
        {
            if (pools == null)
            {
                return false;
            }
            return pools.ContainsKey(address);
        }

        public void RegisterObjectPool(string address, IRecyclable prototype, float timeout = DEFAULT_TIMEOUT)
        {
            if (pools == null)
            {
                pools = new Dictionary<string, Pool<IRecyclable>>();
            }
            if (!pools.ContainsKey(address))
            {
                Pool<IRecyclable> pool = new Pool<IRecyclable>(() =>
                {
                    var obj = GameObject.Instantiate(prototype.GameObject, transform);
                    obj.transform.position = Vector2.one * -10000;
                    var recyclable = obj.GetComponent<IRecyclable>();
                    recyclable.PoolAddress = address;
                    recyclable.OnCreate();
                    return recyclable;
                }, timeout);
                pools.Add(address, pool);
            }
        }

        public void RegisterObjectPool(string address, IRecyclable prototype, Func<IRecyclable, IRecyclable> copyCtor, float timeout = DEFAULT_TIMEOUT)
        {
            if (pools == null)
            {
                pools = new Dictionary<string, Pool<IRecyclable>>();
            }
            if (!pools.ContainsKey(address))
            {
                Pool<IRecyclable> pool = new Pool<IRecyclable>(() => copyCtor(prototype), timeout);
                pools.Add(address, pool);
            }
        }


        public void RegisterObjectPool(string address, Action onInited, float timeout = DEFAULT_TIMEOUT)
        {
            var handle = AddressableManager.Instance.LoadAssetAsync<GameObject>(address);
            handle.Completed += (go) =>
            {
                RegisterRecyclablePoolThenInvokeInit(go, address, onInited, timeout);
            };
        }

        void RegisterRecyclablePoolThenInvokeInit(GameObject go, string address, Action onInited, float timeout = DEFAULT_TIMEOUT)
        {
            if (pools == null)
            {
                pools = new Dictionary<string, Pool<IRecyclable>>();
            }
            if (!pools.ContainsKey(address))
            {
                var prototype = go.GetComponent<IRecyclable>();
                Pool<IRecyclable> pool = new Pool<IRecyclable>(() =>
                {
                    var obj = GameObject.Instantiate(prototype.GameObject, transform);
                    var recyclable = obj.GetComponent<IRecyclable>();
                    recyclable.PoolAddress = address;
                    recyclable.OnCreate();
                    return recyclable;
                }, timeout);
                pools.Add(address, pool);
            }
            onInited?.Invoke();
        }


        public T GetObject<T>(string address) where T : IRecyclable
        {
            if (!pools.TryGetValue(address, out Pool<IRecyclable> pool))
            {
                var error = $"RecyclableManager未注册对象池:{address}";
                GameExtension.Logger.Error(error);
                throw new Exception(error);
            }


            var obj = pool.Get();
            if (!obj.GameObject.activeSelf)
            {
                obj.GameObject.SetActive(true);
            }
            return (T)obj;
        }

        public T GetObjectFromPrototype<T>(string poolAddress) where T : MonoBehaviour, IRecyclable
        {
            if (!HasObjectPool(poolAddress))
            {
                var prototype = PrototypeProvider.Instance.ProviderPrototype<T>();
                RecyclableManager.Instance.RegisterObjectPool(poolAddress, prototype);
            }
            return GetObject<T>(poolAddress);
        }

        public AsyncGetHandle<T> AsyncGetObjectFromPrototype<T>(string poolAddress) where T : MonoBehaviour, IRecyclable
        {
            AsyncGetHandle<T> handle = new AsyncGetHandle<T>();
            if (!HasObjectPool(poolAddress))
            {
                PrototypeProvider.Instance.ProviderPrototypeHandle<T>().Completed += (result) =>
                {
                    RegisterObjectPool(poolAddress, result);
                    handle.SetResult(GetObject<T>(poolAddress));
                };
            }
            else
            {
                handle.SetResult(GetObject<T>(poolAddress));
            }
            return handle;
        }


        public void ReturnObject(string address, IRecyclable obj)
        {
            if (obj == null)
            {
                var error = $"RecyclableManager尝试回收空对象:{address}";
                GameExtension.Logger.Error(error);
                return;
            }

            if (!pools.TryGetValue(address, out var pool))
            {
                var error = $"RecyclableManager尝试回收对象到未分配的对象池:{address}";
                GameExtension.Logger.Error(error);
                return;
            }

            if (obj.GameObject.activeSelf)
            {
                obj.GameObject.SetActive(false);
            }
            obj.GameObject.transform.SetParent(transform);
            pool.Return(obj);
        }
    }
}

