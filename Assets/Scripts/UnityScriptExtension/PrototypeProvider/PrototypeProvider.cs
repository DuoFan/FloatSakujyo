using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameExtension
{
    public class PrototypeProvider : SingletonMonoBase<PrototypeProvider>,IGameInitializer
    {

        Dictionary<Type, GameObject> prototypeDict;

        [SerializeField]
        PrototypeEntry[] prototypeEntries;

        [SerializeField]
        string preloadPrototypeAddress;
        [SerializeField]
        Vector3 prototypePosition;

        public IEnumerator InitializeGame()
        {
            prototypeDict = new Dictionary<Type, GameObject>();

            if (!string.IsNullOrEmpty(preloadPrototypeAddress))
            {
                var handle = AddressableManager.Instance.LoadAssetsAsync<GameObject>(preloadPrototypeAddress);
                yield return handle.WaitForCompletion();
                for (var i = 0; i < handle.Result.Count; i++)
                {
                    PutPrototype(handle.Result[i]);
                }
            }
            else
            {
                yield break;
            }
        }

        void PutPrototype(GameObject prototype)
        {
            var go = GameObject.Instantiate(prototype);
            go.transform.SetParent(transform);
            go.transform.position = prototypePosition;
            PrototypeProvideComponentRef uIProvideComponentRef = go.GetComponent<PrototypeProvideComponentRef>();
            prototypeDict.Add(uIProvideComponentRef.ComponentRef.GetType(), go);
            GameObject.Destroy(uIProvideComponentRef);
        }

        public AsyncGetHandle<T> ProviderPrototypeHandle<T>() where T : MonoBehaviour
        {
            AsyncGetHandle<T> handle = new AsyncGetHandle<T>();
            if (prototypeDict.TryGetValue(typeof(T), out GameObject ui))
            {
                handle.SetResult(ui.GetComponent<T>());
            }
            else
            {
                PrototypeEntry entry = default;
                var typeFullName = typeof(T).FullName;
                foreach (var _entry in prototypeEntries)
                {
                    if (_entry.typeFullName == typeFullName)
                    {
                        entry = _entry;
                    }
                }

                if (entry.address != null)
                {
                    var addressHandle = AddressableManager.Instance.LoadAssetAsync<GameObject>(entry.address);
                    addressHandle.Completed += PutPrototype;
                    addressHandle.Completed += (x) =>
                    {
                        prototypeDict.TryGetValue(typeof(T), out GameObject ui);
                        handle.SetResult(ui.GetComponent<T>());
                    };
                }
                else
                {
                    var error = $"未找到{typeFullName}的PrototypeEntry";
                }
            }
            return handle;
        }

        public T ProviderPrototype<T>() where T : MonoBehaviour
        {
            if (prototypeDict.TryGetValue(typeof(T), out GameObject ui))
            {
                return ui.GetComponent<T>();
            }
            else
            {
                return null;
            }
        }

        public T ProvideInstance<T>() where T : MonoBehaviour
        {
            if (prototypeDict.TryGetValue(typeof(T), out GameObject ui))
            {
                return GameObject.Instantiate(ui).GetComponent<T>();
            }
            else
            {
                return null;
            }
        }

        [Serializable]
        struct PrototypeEntry
        {
            public string address;
            public string typeFullName;
        }
    }
}

