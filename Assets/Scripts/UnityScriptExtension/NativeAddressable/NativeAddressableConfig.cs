using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameExtension
{
    [CreateAssetMenu(fileName = "NativeAddressableConfig", menuName = "NativeAddressableConfig", order = 0)]
    public class NativeAddressableConfig : ScriptableObject
    {
        [SerializeField]
        NativeAddressableAsset[] assets;
        public NativeAddressableAsset[] Assets => assets;
        [SerializeField]
        NativeAddressableAssets[] assetsLists;
        public NativeAddressableAssets[] AssetsLists => assetsLists;


        private void Awake()
        {
            if(assets == null)
            {
                assets = new NativeAddressableAsset[0];
            }
            if(assetsLists == null)
            {
                assetsLists = new NativeAddressableAssets[0];
            }
        }

        public void AddAssets(string address,string group,params Object[] objs)
        {
            NativeAddressableAssets assetsList = null;
            for (int i = 0; i < AssetsLists.Length; i++)
            {
                if (AssetsLists[i].address == address)
                {
                    assetsList = AssetsLists[i];
                    break;
                }
            }

            if(assetsList != null)
            {
                List<NativeAddressableAsset> list = new List<NativeAddressableAsset>(assetsList.assets);
                for (int i = 0; i < objs.Length; i++)
                {
                    bool isExist = list.Exists((item) => item.asset == objs[i]);
                    if (!isExist)
                    {
                        NativeAddressableAsset newAsset = new NativeAddressableAsset();
                        newAsset.address = address;
                        newAsset.asset = objs[i];
                        newAsset.group = group;
                        list.Add(newAsset);
                    }
                }
                assetsList.assets = list.ToArray();
                return;
            }

            NativeAddressableAsset asset = null;
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i].address == address)
                {
                    asset = assets[i];
                    break;
                }
            }

            if (asset == null && objs.Length == 1)
            {
                asset = new NativeAddressableAsset();
                asset.address = address;
                asset.asset = objs[0];
                asset.group = group;
                var tempList = new List<NativeAddressableAsset>(assets);
                tempList.Add(asset);
                assets = tempList.ToArray();
            }
            else
            {
                RemoveAssets(address, asset.asset);
                var list = new List<NativeAddressableAsset>();
                list.Add(asset);
                for (int i = 0; i < objs.Length; i++)
                {
                    bool isExist = list.Exists((item) => item.asset == objs[i]);
                    if (!isExist)
                    {
                        NativeAddressableAsset newAsset = new NativeAddressableAsset();
                        newAsset.address = address;
                        newAsset.asset = objs[i];
                        newAsset.group = group;
                        list.Add(newAsset);
                    }
                }
                NativeAddressableAssets nativeAddressableAssets = new NativeAddressableAssets();
                nativeAddressableAssets.address = address;
                nativeAddressableAssets.assets = list.ToArray();
                var tempList = new List<NativeAddressableAssets>(assetsLists);
                tempList.Add(nativeAddressableAssets);
                assetsLists = tempList.ToArray();
            }
        }
        public void RemoveAssets(string address, params Object[] objs)
        {
            NativeAddressableAssets assetsList = null;
            for (int i = 0; i < AssetsLists.Length; i++)
            {
                if (AssetsLists[i].address == address)
                {
                    assetsList = AssetsLists[i];
                    break;
                }
            }

            if (assetsList != null)
            {
                List<NativeAddressableAsset> list = new List<NativeAddressableAsset>(assetsList.assets);
                for (int i = 0; i < objs.Length; i++)
                {
                    var beRemove = list.Find((item) => item.asset == objs[i]);
                    if (beRemove != null)
                    {
                        list.Remove(beRemove);
                    }
                }
                if(list.Count > 0)
                {
                    assetsList.assets = list.ToArray();
                }
                else
                {
                    List<NativeAddressableAssets> tempList = new List<NativeAddressableAssets>(assetsLists);
                    tempList.Remove(assetsList);
                    assetsLists = tempList.ToArray();
                }
                return;
            }

            NativeAddressableAsset asset = null;
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i].address == address)
                {
                    asset = assets[i];
                    break;
                }
            }

            if (asset != null)
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    if (asset.asset == objs[i])
                    {
                        List<NativeAddressableAsset> tempList = new List<NativeAddressableAsset>(assets);
                        tempList.Remove(asset);
                        assets = tempList.ToArray();
                        return;
                    }
                }
            }
        }

        [Serializable]
        public class NativeAddressableAsset
        {
            public string address;
            public string group;
            public Object asset;
        }

        [Serializable]
        public class NativeAddressableAssets
        {
            public string address;
            public NativeAddressableAsset[] assets;
        }
    }
}
