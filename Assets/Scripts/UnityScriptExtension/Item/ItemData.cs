using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace GameExtension
{
    [System.Serializable]
    public class ItemData : IConfigData, IRareHolder, IIconAddressHolder
    {
        public int ID { get; private set; }
        public string ItemName { get; private set; }
        public string Description { get; private set; } // 物品用途说明
        public string IconAddress { get; private set; }
        public string DataName => ItemName;

        public RareData Rare { get; private set; }

        [JsonConstructor]
        public ItemData(int iD, string itemName, string description, string iconAddress, RareData rare)
        {
            ID = iD;
            ItemName = itemName;
            Description = description;
            IconAddress = iconAddress;
            Rare = rare;
        }

        public LoadAssetHandle<Sprite> LoadIconForImage(Image image)
        {
            var handle = AddressableManager.Instance.LoadAssetAsync<Sprite>(IconAddress);
            handle.Completed += (x) =>
            {
                if (image != null)
                {
                    image.sprite = x;
                    image.SetNativeSize();
                }
                else
                {
                    GameExtension.Logger.Log("加载过程中Image已被销毁,IconAddress" + IconAddress);
                }
            };
            return handle;


        }
    }
}