using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public interface IExtensionItemAdapter
    {
        ItemData GetItemData(int itemID);
        int GetRandomItemID(RareData rare);
    }
}
