using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class ExtensionItemAdapter : IExtensionItemAdapter
    {
        public ItemData GetItemData(int itemID)
        {
            return ItemDataManagerBase.Instance.GetData(itemID);
        }

        //ItemData默认不会有稀有度
        public int GetRandomItemID(RareData rare)
        {
            return -1;
        }
    }
}

