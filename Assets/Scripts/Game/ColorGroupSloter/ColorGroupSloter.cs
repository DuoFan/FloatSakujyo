using GameExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloatSakujyo.Game
{
    public class ColorGroupSloter : IDisposable
    {
        const int NONE_COLOR_GROUP_SLOT_INDEX = 0;
        public List<ItemColor> ColorGroupQueues { get; private set; }

        public ColorGroupSlot[] ColorGroupSlots { get; private set; }
        public int GeneratingSlotGroupCount { get; private set; }

        Dictionary<ItemColor, List<Item>> backupItems;

        int groupSlotIndex = NONE_COLOR_GROUP_SLOT_INDEX;

        public event Action<ColorGroupSlot> OnSlotCompleted;
        public event Action<ColorGroupSlot> OnSlotGenerated;

        bool isGroupDefaultUseable;

        public ColorGroupSloter(List<ItemColor> colorGroupQueues, int defaultNoneGroupSlotCount, bool isGroupDefaultUseable)
        {
            ColorGroupQueues = colorGroupQueues;
            ColorGroupSlots = new ColorGroupSlot[3];

            this.isGroupDefaultUseable = isGroupDefaultUseable;

            backupItems = new Dictionary<ItemColor, List<Item>>();

            //空颜色有5个槽位
            ColorGroupSlots[NONE_COLOR_GROUP_SLOT_INDEX] = new ColorGroupSlot(ItemColor.None, defaultNoneGroupSlotCount, isGroupDefaultUseable);
            //初始化两组颜色
            for (int i = 0; i < 2; i++)
            {
                if (ColorGroupQueues.Count > 0)
                {
                    NextGroup();
                }
            }
        }

        public void AddBackupItem(Item item)
        {
            if (!backupItems.TryGetValue(item.ItemColor, out var itemList))
            {
                itemList = new List<Item>();
                backupItems[item.ItemColor] = itemList;
            }
            itemList.Add(item);
        }

        public void NextGroup(ItemColor itemColor = ItemColor.None)
        {
            groupSlotIndex++;
            if (groupSlotIndex >= ColorGroupSlots.Length)
            {
                groupSlotIndex = NONE_COLOR_GROUP_SLOT_INDEX + 1;
            }
            NextGroup(groupSlotIndex, itemColor);
        }

        public void NextGroup(int groupSlotIndex, ItemColor itemColor = ItemColor.None)
        {
            var index = 0;
            bool isOk = true;
            for (int i = 0; i < ColorGroupQueues.Count; i++)
            {
                isOk = true;
                if (itemColor == ItemColor.None)
                {
                    var _itemColor = ColorGroupQueues[i];
                    for (int j = 0; j < ColorGroupSlots.Length; j++)
                    {
                        if (ColorGroupSlots[j] != null && ColorGroupSlots[j].ItemColor == _itemColor && ColorGroupSlots[j].HasEmptySlot())
                        {
                            isOk = false;
                            break;
                        }
                    }
                }
                else
                {
                    isOk = ColorGroupQueues[i] == itemColor;
                }

                if (isOk)
                {
                    break;
                }
                else
                {
                    index++;
                }
            }

            ColorGroupSlot slot;

            if (ColorGroupQueues.Count > 0)
            {
                index = isOk ? index : 0;
                //其他颜色有3个槽位
                slot = new ColorGroupSlot(ColorGroupQueues[index], 3, isGroupDefaultUseable);
                ColorGroupQueues.RemoveAt(index);
            }
            else
            {
                slot = new ColorGroupSlot(ItemColor.Color1, 3, isGroupDefaultUseable);
            }

            ColorGroupSlots[groupSlotIndex] = slot;

            if (backupItems.TryGetValue(slot.ItemColor, out var screwList))
            {
                for (int i = screwList.Count - 1; i >= 0 && slot.HasEmptySlot(); i--)
                {
                    slot.FillItem(screwList[i], slot.AllocateIndexForItem(screwList[i]), out var anim);
                    slot.CompleteFillItem();
                    screwList.RemoveAt(i);
                }
                if (screwList.Count <= 0)
                {
                    backupItems.Remove(slot.ItemColor);
                }
            }
        }

        public ColorGroupSlot FindUseableSlotForColor(ItemColor screwColor)
        {
            var groupSlot = FindUseableColorGroupSlot(screwColor);
            if (groupSlot == null)
            {
                groupSlot = FindUseableColorGroupSlot(ItemColor.None);
            }
            return groupSlot;
        }

        ColorGroupSlot FindUseableColorGroupSlot(ItemColor screwColor)
        {
            for (int i = 0; i < ColorGroupSlots.Length; i++)
            {
                var slot = ColorGroupSlots[i];
                if (slot != null && slot.ItemColor == screwColor && slot.HasEmptySlot() && slot.Useable)
                {
                    return slot;
                }
            }

            return null;
        }

        public ColorGroupSlot FindColorGroupSlot(ItemColor screwColor)
        {
            for (int i = 0; i < ColorGroupSlots.Length; i++)
            {
                var slot = ColorGroupSlots[i];
                if (slot != null && slot.ItemColor == screwColor)
                {
                    return slot;
                }
            }

            return null;
        }

        public void Dispose()
        {
            for (int i = 0; i < ColorGroupSlots.Length; i++)
            {
                ColorGroupSlots[i]?.Dispose();
            }
            ColorGroupSlots = null;
            ColorGroupQueues = null;
        }

        public void AddGeneratingSlotGroup()
        {
            GeneratingSlotGroupCount++;
        }

        public void RemoveCompletingSlotGroup()
        {
            GeneratingSlotGroupCount--;
        }

        public bool TryCompleteSlot()
        {
            bool result = false;
            for (int i = 0; i < ColorGroupSlots.Length; i++)
            {
                var slot = ColorGroupSlots[i];
                if (slot == null)
                {
                    continue;
                }
                if (!slot.HasEmptySlot())
                {
                    //没有正在填充的螺丝
                    if (slot.ItemColor != ItemColor.None && slot.FillingItemCount == 0)
                    {
                        result = true;
                        OnSlotCompleted?.Invoke(ColorGroupSlots[i]);
                        if (ColorGroupQueues.Count > 0)
                        {
                            NextGroup(i);
                            OnSlotGenerated?.Invoke(ColorGroupSlots[i]);
                        }
                        else
                        {
                            ColorGroupSlots[i] = null;
                        }
                    }
                }
            }

            return result;
        }

        public void AdjustNoneGroupSlot(ColorGroupSlot colorGroupSlot)
        {
            var noneGroupSlot = FindColorGroupSlot(ItemColor.None);

            void AdjustNoneGroupSlot()
            {
                for (int j = 0; j < noneGroupSlot.Items.Length; j++)
                {
                    var item = noneGroupSlot.Items[j];
                    if (item != null && item.ItemColor == colorGroupSlot.ItemColor)
                    {
                        if (colorGroupSlot.HasEmptySlot())
                        {
                            noneGroupSlot.RemoveItem(j);

                            colorGroupSlot.FillItem(item, colorGroupSlot.AllocateIndexForItem(item), out var fillAnimation);

                            bool isFilled = !colorGroupSlot.HasEmptySlot();

                            if (fillAnimation != null)
                            {
                                IEnumerator WaitAnimationThemComplete()
                                {
                                    yield return fillAnimation;
                                    colorGroupSlot.CompleteFillItem();
                                    if (isFilled)
                                    {
                                        if (colorGroupSlot.FillingItemCount > 0)
                                        {
                                            yield return new WaitUntil(() => colorGroupSlot.FillingItemCount <= 0);
                                        }
                                        TryCompleteSlot();
                                    }
                                }
                                GameController.Instance.StartCoroutine(WaitAnimationThemComplete());
                            }
                            else
                            {
                                colorGroupSlot.CompleteFillItem();
                                if (!colorGroupSlot.HasEmptySlot())
                                {
                                    TryCompleteSlot();
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            if (noneGroupSlot.FillingItemCount > 0)
            {
                IEnumerator WaitForAdjustNoneGroupSlot()
                {
                    yield return new WaitUntil(() => noneGroupSlot.FillingItemCount <= 0);
                    AdjustNoneGroupSlot();
                }
                GameController.Instance.StartCoroutine(WaitForAdjustNoneGroupSlot());
            }
            else
            {
                AdjustNoneGroupSlot();
            }
        }

        public ColorGroupSlot OpenNewGroup(ItemColor itemColor = ItemColor.None)
        {
            var newColorGroupSlots = new ColorGroupSlot[ColorGroupSlots.Length + 1];
            for (int i = 0; i < ColorGroupSlots.Length; i++)
            {
                newColorGroupSlots[i] = ColorGroupSlots[i];
            }

            ColorGroupSlots = newColorGroupSlots;

            NextGroup(ColorGroupSlots.Length - 1, itemColor);
            return ColorGroupSlots[ColorGroupSlots.Length - 1];
        }

        public void AddNoneSlot()
        {
            ColorGroupSlots[NONE_COLOR_GROUP_SLOT_INDEX].AddSlot();
        }

        //找到当前最紧急的颜色
        public ItemColor FindUrgentColor(IEnumerable<Item> items)
        {
            var noneSlot = FindColorGroupSlot(ItemColor.None);

            //减去None
            int[] itemColors = new int[(int)ItemColor.Color10];
            int maxColorCount = 0;
            ItemColor itemColor = ItemColor.None;

            for (int i = 0; i < noneSlot.Items.Length; i++)
            {
                if (noneSlot.Items[i] != null)
                {
                    itemColors[(int)noneSlot.Items[i].ItemColor - 1]++;
                    if (itemColors[(int)noneSlot.Items[i].ItemColor - 1] > maxColorCount)
                    {
                        maxColorCount = itemColors[(int)noneSlot.Items[i].ItemColor - 1];
                        itemColor = noneSlot.Items[i].ItemColor;
                    }
                }
            }

            /*if (itemColor == ItemColor.None)
            {
                var levelEntity = GameController.Instance.LevelEntity;
                int easyDifficult = 0;
                IScrew item = null;
                foreach (var _item in items)
                {
                    if (item == null)
                    {
                        item = _item;
                        easyDifficult = item.Hole.Difficulty;
                    }
                    else if (_item.Hole.Difficulty < easyDifficult)
                    {
                        item = _item;
                        easyDifficult = item.Hole.Difficulty;
                    }
                }

                if (item != null)
                {
                    itemColor = item.ScrewColor;
                }
            }*/

            return itemColor;
        }
    }
}
