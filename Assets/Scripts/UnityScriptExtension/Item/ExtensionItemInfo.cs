using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace GameExtension
{
    public static partial class ExtensionItemType
    {
        public const int ITEM = 0;
    }

    public abstract class ExtensionItemInfoBase
    {
        public int ItemID { get; protected set; }
        public ItemData ItemData
        {
            get
            {
                if (itemData == null || itemData.ID != ItemID)
                {
                    itemData = adpter.GetItemData(ItemID);
                }
                return itemData;
            }
        }
        ItemData itemData;
        public int ItemType { get; protected set; }
        protected IExtensionItemAdapter adpter;
        public ExtensionItemInfoBase(int extensionItemType, int itemID)
        {
            ItemType = extensionItemType;
            ItemID = itemID;
            adpter = ExtensionItemAdapterFactory.GetExtensionItemAdapter(extensionItemType);
        }
    }


    public class ExtensionItemAmountInfo : ExtensionItemInfoBase
    {
        public int ID => ItemID;
        public BigInteger Amount => ItemAmount;
        public BigInteger ItemAmount { get; set; }

        public ExtensionItemAmountInfo(int extensionItemType, int itemID, BigInteger itemAmount) : base(extensionItemType, itemID)
        {
            ItemAmount = itemAmount;
        }

        public void Adapt(int id, int amount)
        {
            ItemID = id;
            ItemAmount = amount;
        }
    }

    public class ExtensionItemAddInfo : ExtensionItemInfoBase, IIDAmountInfo
    {
        public int ID => ItemID;
        public virtual int Amount => ItemAddAmount;
        public int ItemAddAmount { get; private set; }

        public ExtensionItemAddInfo(int extensionItemType, int itemID, int itemAmount) : base(extensionItemType, itemID)
        {
            this.ItemAddAmount = itemAmount;
        }

        public void Adapt(int id, int amount)
        {
            ItemID = id;
            ItemAddAmount = amount;
        }
    }
    public class ExtensionRandomItemInfo : ExtensionItemInfoBase, IRareHolder, IAmountProvider
    {
        public int Amount { get; private set; }
        public RareData Rare { get; private set; }

        protected IExtensionItemAdapter adapter;

        public ExtensionRandomItemInfo(int extensionItemType, int amount, RareData rare) : base(extensionItemType, -1)
        {
            Amount = amount;
            Rare = rare;
            adapter = ExtensionItemAdapterFactory.GetExtensionItemAdapter(ItemType);
        }

        public void FixItem()
        {
            ItemID = adapter.GetRandomItemID(Rare);
        }
    }
}
