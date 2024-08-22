using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

namespace FloatSakujyo.Game
{
    public class ColorGroupSlot : IDisposable
    {
        public ItemColor ItemColor;
        public int TotalSlotCount { get; private set; }
        public int EmptySlotCount
        {
            get
            {
                var count = 0;
                for (int i = 0; i < Items.Length; i++)
                {
                    if (Items[i] == null)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public Item[] Items { get; private set; }

        public event Func<Item, int, IEnumerator> OnFillItem;
        public event Action<Item, int> OnRemoveItem;
        public event Action OnAddSlot;

        public int FillingItemCount { get; private set; }

        public bool Useable;

        public ColorGroupSlot(ItemColor itemColor, int emptySlotCount, bool useable)
        {
            ItemColor = itemColor;
            TotalSlotCount = emptySlotCount;
            Items = new Item[TotalSlotCount];
            Useable = useable;
        }

        public void SetUseable(bool useable)
        {
            Useable = useable;
        }

        public bool HasEmptySlot()
        {
            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i] == null)
                {
                    return true;
                }
            }
            return false;
        }

        public int AllocateIndexForItem(Item item)
        {
            int index = 0;
            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i] == null)
                {
                    Items[i] = item;
                    index = i;
                    break;
                }
            }
            FillingItemCount++;
            return index;
        }

        public void FillItem(Item item, int index, out IEnumerator fillAnimation)
        {
            Items[index] = item;
            fillAnimation = OnFillItem?.Invoke(item, index);
        }

        public void CompleteFillItem()
        {
            FillingItemCount--;
        }


        public void RemoveItem(int index)
        {
            var item = Items[index];
            Items[index] = null;
            OnRemoveItem?.Invoke(item, index);
        }

        public void Dispose()
        {
            OnFillItem = null;
            OnAddSlot = null;
            OnRemoveItem = null;

            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i] != null && Items[i] is MonoBehaviour mono)
                {
                    GameObject.Destroy(mono.gameObject);
                }
            }
            Items = null;
        }

        public void AddSlot()
        {
            TotalSlotCount++;
            var newScrews = Items;
            Array.Resize(ref newScrews, TotalSlotCount);
            Items = newScrews;
            OnAddSlot?.Invoke();
        }
    }
}
