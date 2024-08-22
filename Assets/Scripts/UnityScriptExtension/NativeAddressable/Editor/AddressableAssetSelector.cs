using EditorExtension;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension.Editor
{
    public class AddressableAssetSelector<T> : ISetter<string> where T : Object
    {
        public T asset;
        public T oldAsset;
        public event System.Action<T> OnAssetChange;
        public void Set(string address)
        {
            asset = EditorExtension.EditorUtils.LoadAddressableAsset<T>(address);
            oldAsset = asset;
            OnAssetChange?.Invoke(asset);
        }
        public bool TrySaveToAddressEntry(string address, string groupName = null)
        {

            if (oldAsset != null && oldAsset != asset)
            {
                EditorExtension.EditorUtils.RemoveAddressableAssetEntry(oldAsset);
            }

            if (asset != null)
            {
                var entry = EditorExtension.EditorUtils.GetAddressableAssetEntry(asset);
                if (entry == null)
                {
                    if (groupName == null)
                    {
                        entry = EditorExtension.EditorUtils.CreateEntryToGroupByGroupName(asset, groupName);
                    }
                    else
                    {
                        entry = EditorExtension.EditorUtils.CreateEntryToGroup(asset);
                    }
                }
                entry.address = address;
            }
            return asset != null;
        }

        public void Draw()
        {
            asset = (T)UnityEditor.EditorGUILayout.ObjectField(asset, typeof(T), false);
            if (GUI.changed)
            {
                OnAssetChange?.Invoke(asset);
            }
        }
    }
}
