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

        public override IEnumerator Init(LevelData levelData)
        {
            LevelData = levelData;

            ColorGroupSloter = LevelUtils.GenerateColorQueueSloter(levelData, true, out var _colorGroupQueues);

            colorGroupQueues = new Queue<ItemColor>(_colorGroupQueues);

            totalItemCount = colorGroupQueues.Count * 3;

            itemCount = totalItemCount;

            yield break;
        }

        public Item[] SpawnItems(int spawnCount, ItemGeneration generation)
        {
            var items = new Item[spawnCount];
            for (int i = 0; i < spawnCount && colorGroupQueues.Count > 0; i++)
            {
                var itemColor = colorGroupQueues.Peek();
                var itemConfigData = ItemColorConfigDataManager.Instance.GetConfigData(itemColor);
                var item = Instantiate(itemConfigData.Item, transform);
                item.SetGeneration(generation);

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
            //isFaild = false;
            //有可能在失败后填满了某一个盒子
            ColorGroupSloter.TryCompleteSlot();
        }

        public override void Dispose()
        {
            ColorGroupSloter.Dispose();
            base.Dispose();
        }
    }
}
