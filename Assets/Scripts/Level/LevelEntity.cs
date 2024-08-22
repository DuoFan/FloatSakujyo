using DG.Tweening;
using GameExtension;
using FloatSakujyo.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Device;

namespace FloatSakujyo.Level
{
    public class LevelEntity : LevelEntityBase
    {
        Queue<ItemColor> colorGroupQueues;
        int spawnCounter;

        public ColorGroupSloter ColorGroupSloter { get; private set; }

        public int itemCount;
        int totalItemCount;

        bool isFaild;
        bool isCompleted;

        public event Action OnGameFailed;
        public event Action OnGameCompleted;

        public Queue<Item> EdenItems { get; private set; }
        public Queue<Item> MidItems { get; private set; }
        public HashSet<Item> OldItems { get; private set; }

        public event Action<Item> OnNewEden;
        public event Action<Item> OnEdenToMid;
        public event Action<Item> OnMidToOld;
        public event Action<Item> OnOldToDie;
        public event Action<Item> OnItemTook;

        public override IEnumerator Init(LevelData levelData)
        {
            LevelData = levelData;

            ColorGroupSloter = LevelUtils.GenerateColorQueueSloter(levelData, true, out var _colorGroupQueues);

            colorGroupQueues = new Queue<ItemColor>(_colorGroupQueues);

            totalItemCount = colorGroupQueues.Count * 3;

            itemCount = totalItemCount;

            EdenItems = new Queue<Item>();
            MidItems = new Queue<Item>();
            OldItems = new HashSet<Item>();

            SpawnItems(levelData.EdenItemCount, ItemGeneration.Eden);
            SpawnItems(levelData.MidItemCount, ItemGeneration.Mid);
            SpawnItems(levelData.OldItemCount, ItemGeneration.Old);

            yield break;
        }

        public Item[] SpawnItems(int spawnCount, ItemGeneration generation)
        {
            var items = new Item[spawnCount];
            for (int i = 0; i < spawnCount; i++)
            {
                var itemColor = colorGroupQueues.Peek();
                var itemConfigData = ItemColorConfigDataManager.Instance.GetConfigData(itemColor);
                var item = Instantiate(itemConfigData.Item, transform);
                item.SetGeneration(generation);

                switch (generation)
                {
                    case ItemGeneration.Eden:
                        EdenItems.Enqueue(item);
                        break;
                    case ItemGeneration.Mid:
                        MidItems.Enqueue(item);
                        break;
                    case ItemGeneration.Old:
                        OldItems.Add(item);
                        break;
                }

                items[i] = item;

                spawnCounter++;

                if (spawnCounter >= 3)
                {
                    colorGroupQueues.Dequeue();
                    spawnCounter = 0;
                }
            }

            return items;
        }

        public override bool TryTakeItem(Item item)
        {
            var noneSlot = ColorGroupSloter.FindColorGroupSlot(ItemColor.None);
            //noneSlot没有空槽位时说明游戏即将失败
            if (!noneSlot.HasEmptySlot())
            {
                return false;
            }

            var slot = ColorGroupSloter.FindUseableSlotForColor(item.ItemColor);
            if (slot == null)
            {
                return false;
            }

            StartCoroutine(IETakeScrew(slot, item));

            return true;
        }

        IEnumerator IETakeScrew(ColorGroupSlot slot, Item item)
        {
            OldItems.Remove(item);

            OnOldToDie(item);

            int index = slot.AllocateIndexForItem(item);

            bool isSlotFilled = !slot.HasEmptySlot();

            slot.FillItem(item, index, out var fillAnimation);
            yield return fillAnimation;
            slot.CompleteFillItem();

            OnItemTook?.Invoke(item);

            if(MidItems.Count > 0)
            {
                var midItem = MidItems.Dequeue();
                OldItems.Add(midItem);
                OnMidToOld?.Invoke(midItem);
            }

            if(EdenItems.Count > 0)
            {
                var edenItem = EdenItems.Dequeue();
                MidItems.Enqueue(edenItem);
                OnEdenToMid?.Invoke(edenItem);
            }

            if (colorGroupQueues.Count > 0)
            {
                var edenItem = SpawnItems(1, ItemGeneration.Eden)[0];
                OnNewEden?.Invoke(edenItem);
            }


            if (isSlotFilled)
            {
                if (slot.FillingItemCount > 0)
                {
                    yield return new WaitUntil(() => slot.FillingItemCount <= 0);
                }

                ColorGroupSloter.TryCompleteSlot();

                if (slot.ItemColor == ItemColor.None && !isFaild)
                {
                    //等待生成的槽位完成
                    yield return new WaitUntil(() => ColorGroupSloter.GeneratingSlotGroupCount <= 0);

                    //槽位满了则失败
                    if (!slot.HasEmptySlot() && !isFaild)
                    {
                        isFaild = true;
                        OnGameFailed?.Invoke();
                    }
                }
            }

            if (itemCount == 0 && !isFaild && !isCompleted)
            {
                isCompleted = true;
                OnGameCompleted?.Invoke();
            }

            yield break;
        }

        public void ClearGroupSlot(ColorGroupSlot groupSlot)
        {
            var screws = groupSlot.Items.Clone() as Item[];
            for (int i = 0; i < groupSlot.Items.Length; i++)
            {
                if (groupSlot.Items[i] != null)
                {
                    groupSlot.RemoveItem(i);
                }
            }
            StartCoroutine(BackupScrews(screws));
        }

        //后备螺丝，用于后续盒子使用
        IEnumerator BackupScrews(IEnumerable<Item> items)
        {/*
            foreach (Item item in items)
            {
                if (item != null)
                {
                    item.transform.SetParent(content);
                }
            }

            var time = 1f;
            foreach (Item item in items)
            {
                if (item != null)
                {
                    //还未拔起的螺丝
                    if (item.Hole != null)
                    {
                        yield return item.ReleaseHole();
                        Screws.Remove(item);
                        OnScrewTook?.Invoke();
                    }
                    item.transform.SetParent(disposedContent);
                    item.transform.DOMove(transform.position + Vector3.up * 100, time);
                    ColorGroupSloter.AddBackupScrew(item);
                }
            }*/

            yield break;
        }

        public float GetProgress()
        {
            return 1 - (float)itemCount / totalItemCount;
        }

        public void Restore()
        {
            isFaild = false;
            //有可能在失败后填满了某一个盒子
            ColorGroupSloter.TryCompleteSlot();
        }

        public override void Dispose()
        {
            OnItemTook = null;
            OnGameFailed = null;
            OnGameCompleted = null;
            ColorGroupSloter.Dispose();
            base.Dispose();
        }
    }
}
