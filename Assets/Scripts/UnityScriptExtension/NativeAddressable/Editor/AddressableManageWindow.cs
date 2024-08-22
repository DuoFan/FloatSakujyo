using EditorExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Experimental.GlobalIllumination;
using static GameExtension.NativeAddressableConfig;
using Object = UnityEngine.Object;

namespace GameExtension.Editor
{
    public class AddressableManageWindow : UnityEditor.EditorWindow
    {
        static AddressableManageWindow window;
        [UnityEditor.MenuItem("GameExtension/Windows/AddressableManageWindow")]
        public static void Open()
        {
            window = UnityEditor.EditorWindow.GetWindow<AddressableManageWindow>();
        }

        NativeAddressableConfig nativeAddressableConfig;
        ObjectDisplayer<ManageEntry> entryDisplayer;
        Vector2 scrollPos;
        private void OnEnable()
        {
            nativeAddressableConfig = AssetUtil.LoadNativeAddressableConfig();
            Dictionary<string, ManageEntry> addressableEntries = new Dictionary<string, ManageEntry>();
            for (int i = 0; i < nativeAddressableConfig.Assets.Length; i++)
            {
                var nativeAsset = nativeAddressableConfig.Assets[i];
                var address = nativeAsset.address;
                var asset = nativeAsset.asset;
                var group = nativeAsset.group;
                addressableEntries.Add(address, new ObjectEntry(address, group, asset, true));
            }
            for (int i = 0; i < nativeAddressableConfig.AssetsLists.Length; i++)
            {
                var nativeAssets = nativeAddressableConfig.AssetsLists[i];
                addressableEntries.Add(nativeAssets.address, new ListEntry(nativeAssets));
            }


            var settings = AddressableAssetSettingsDefaultObject.Settings;
            for (int i = 0; i < settings.groups.Count; i++)
            {
                var group = settings.groups[i];
                if(group.Name.Equals("Built In Data"))
                {
                    continue;
                }
                var entries = group.entries.ToArray();
                for (int j = 0; j < entries.Length; j++)
                {
                    var entry = entries[j];
                    if (addressableEntries.ContainsKey(entry.address))
                    {
                        SloveConflict(addressableEntries, entry);
                    }
                    else
                    {
                        addressableEntries.Add(entry.address, new ObjectEntry(entry.address, entry.parentGroup.Name, entry.MainAsset, false));
                    }
                }
            }

            entryDisplayer = new ObjectDisplayer<ManageEntry>((x, y) =>
            {
                bool isMatch = y.IsMatch(x.address);
                if(!isMatch && (x is ObjectEntry entry))
                {
                    isMatch = y.IsMatch(entry.group);
                }
                return isMatch;
            }, (x, y) =>
            {
                if (x.GetType() == y.GetType())
                {
                    return x.address.CompareTo(y.address);
                }
                else
                {
                    return x is ListEntry ? -1 : 1;
                }
            });
            entryDisplayer.SetObjects(addressableEntries.Values.ToArray());
            entryDisplayer.SetName("资源项");
        }

        void SloveConflict(Dictionary<string, ManageEntry> addressableEntries, AddressableAssetEntry entry)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            var managedEntry = addressableEntries[entry.address];
            if (managedEntry is ObjectEntry objectEntry)
            {
                //说明该资源已经被设置为本地资源,从Addressable中移除
                if (objectEntry.asset.Equals(entry.MainAsset) && objectEntry.IsNative)
                {
                    settings.RemoveAssetEntry(entry.guid);
                }
                //说明该Address包含多个资源
                else
                {
                    addressableEntries.Remove(entry.address);
                    var listEntry = new ListEntry(entry.address);
                    listEntry.AddAsset(objectEntry.asset, objectEntry.group, objectEntry.IsNative);
                    listEntry.AddAsset(entry.MainAsset, entry.parentGroup.Name, false);
                    addressableEntries.Add(entry.address, listEntry);
                }
            }
            else if (managedEntry is ListEntry listEntry)
            {
                //说明该资源已经被设置为本地资源,从Addressable中移除
                if (listEntry.assetEntries.Exists((x) => x.asset.Equals(entry.MainAsset)))
                {
                    settings.RemoveAssetEntry(entry.guid);
                }
                //说明该Address包含多个资源
                else
                {
                    listEntry.AddAsset(entry.MainAsset, entry.parentGroup.Name, false);
                }
            }
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            entryDisplayer.Draw();
            EditorGUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("全部设为本地"))
            {
                AssetDatabase.StartAssetEditing();
                entryDisplayer.ForeachFilter((x) =>
                {
                    if (x is ObjectEntry objectEntry)
                    {
                        objectEntry.SetNative(true);
                    }
                    else if (x is ListEntry listEntry)
                    {
                        for (int i = 0; i < listEntry.assetEntries.Count; i++)
                        {
                            listEntry.assetEntries[i].SetNative(true);
                        }
                    }
                });
                AssetDatabase.StopAssetEditing();
            }
            else if(GUILayout.Button("全部设为远程"))
            {
                AssetDatabase.StartAssetEditing();
                entryDisplayer.ForeachFilter((x) =>
                {
                    if (x is ObjectEntry objectEntry)
                    {
                        objectEntry.SetNative(false);
                    }
                    else if (x is ListEntry listEntry)
                    {
                        for (int i = 0; i < listEntry.assetEntries.Count; i++)
                        {
                            listEntry.assetEntries[i].SetNative(false);
                        }
                    }
                });
                AssetDatabase.StopAssetEditing();
            }
            GUILayout.EndHorizontal();
        }


        abstract class ManageEntry : IDraw
        {
            public string address;
            public abstract void Draw();
        }

        class ObjectEntry : ManageEntry
        {
            public string group;
            public Object asset;
            public bool IsNative => isNative.Value;
            EditorToggle isNative;
            public ObjectEntry(string address, string group, Object asset, bool isNative)
            {
                this.address = address;
                this.asset = asset;
                this.group = group;
                this.isNative = new EditorToggle("设置为本地");
                this.isNative.SetValue(isNative);
                this.isNative.OnValueChange += OnNativeChange;
            }

            public override void Draw()
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Address:{address} Group:{group}");
                EditorGUILayout.ObjectField(asset, typeof(Object), false);
                isNative.Draw();
                EditorGUILayout.EndHorizontal();
            }


            void OnNativeChange(bool isNative)
            {
                if (isNative)
                {
                    window.nativeAddressableConfig.AddAssets(address, group, asset);
                    var entry = EditorExtension.EditorUtils.GetAddressableAssetEntry(asset);
                    if (entry != null)
                    {
                        var settings = AddressableAssetSettingsDefaultObject.Settings;
                        settings.RemoveAssetEntry(entry.guid);
                    }
                }
                else
                {
                    window.nativeAddressableConfig.RemoveAssets(address, asset);
                    var entry = EditorExtension.EditorUtils.CreateEntryToGroupByGroupName(asset, group);
                    entry.address = address;
                }
                EditorUtility.SetDirty(window.nativeAddressableConfig);
                AssetDatabase.SaveAssetIfDirty(window.nativeAddressableConfig);
            }


            public void SetNative(bool isNative)
            {
                this.isNative.SetValue(isNative);
            }
        }
        class ListEntry : ManageEntry
        {
            public List<ObjectEntry> assetEntries;
            bool isFoldout;
            public ListEntry(string address)
            {
                this.address = address;
                assetEntries = new List<ObjectEntry>();
            }
            public ListEntry(NativeAddressableAssets nativeAddressableAssets)
            {
                address = nativeAddressableAssets.address;
                assetEntries = new List<ObjectEntry>();
                for (int i = 0; i < nativeAddressableAssets.assets.Length; i++)
                {
                    var nativeAsset = nativeAddressableAssets.assets[i];
                    assetEntries.Add(new ObjectEntry(address, nativeAsset.group, nativeAsset.asset, true));
                }
            }

            public override void Draw()
            {
                EditorGUILayout.BeginVertical();
                isFoldout = EditorGUILayout.Foldout(isFoldout, address);
                if (isFoldout)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("全部设置为本地", GUILayout.Width(200)))
                    {
                        AssetDatabase.StartAssetEditing();
                        for (int i = 0; i < assetEntries.Count; i++)
                        {
                            assetEntries[i].SetNative(true);
                        }
                        AssetDatabase.StopAssetEditing();
                    }
                    else if (GUILayout.Button("全部设置为远程", GUILayout.Width(200)))
                    {
                        AssetDatabase.StartAssetEditing();
                        for (int i = 0; i < assetEntries.Count; i++)
                        {
                            assetEntries[i].SetNative(false);
                        }
                        AssetDatabase.StopAssetEditing();
                    }
                    EditorGUILayout.EndHorizontal();
                    for (int i = 0; i < assetEntries.Count; i++)
                    {
                        assetEntries[i].Draw();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            public void AddAsset(Object asset, string group, bool isNative)
            {
                assetEntries.Add(new ObjectEntry(address, group, asset, isNative));
            }
        }
    }
}
