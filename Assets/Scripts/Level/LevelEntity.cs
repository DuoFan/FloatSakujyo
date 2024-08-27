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
        int[] leftItemCounts;
        int spawnIndex;

        public ColorGroupSloter ColorGroupSloter { get; private set; }

        public int TotalItemCount { get; private set; }
        public int itemCount;
        int n_RandomThenMaxItem;
        int maxSeriesMaxItemCount;

        public override IEnumerator Init(LevelData levelData)
        {
            LevelData = levelData;

            Array.Sort(levelData.LevelDifficultyDatas, (x, y) => x.LeftItemCount.CompareTo(y.LeftItemCount));

            ColorGroupSloter = LevelUtils.GenerateColorQueueSloter(levelData, 6, true, out var _colorGroupQueues);

            colorGroupQueues = new Queue<ItemColor>(_colorGroupQueues);

            TotalItemCount = colorGroupQueues.Count * 3;

            itemCount = TotalItemCount;

            leftItemCounts = LevelUtils.GetColorGroupCounts(levelData);
            for (int i = 0; i < leftItemCounts.Length; i++)
            {
                leftItemCounts[i] *= 3;
            }

            spawnIndex = TotalItemCount;
            n_RandomThenMaxItem = 0;
            maxSeriesMaxItemCount = 0;

            yield break;
        }

        public Item[] SpawnItems(int spawnCount, ItemGeneration generation)
        {
            var items = new Item[spawnCount];
            for (int i = 0; i < spawnCount && spawnIndex > 0; i++)
            {
                LevelDifficultyData curDifficultyData = null;
                ItemColor itemColor;
                for (int j = 0; j < LevelData.LevelDifficultyDatas.Length; j++)
                {
                    if (spawnIndex <= LevelData.LevelDifficultyDatas[j].LeftItemCount)
                    {
                        curDifficultyData = LevelData.LevelDifficultyDatas[j];
                        break;
                    }
                }

                if (curDifficultyData != null)
                {
                    bool findMax;
                    var randomValue = UnityEngine.Random.Range(0, 100);
                    if ((randomValue < curDifficultyData.MaxGroupRandomValue || n_RandomThenMaxItem >= curDifficultyData.N_RandomThenMaxItem) &&
                        maxSeriesMaxItemCount < curDifficultyData.MaxSeriesMaxItemCount)
                    {
                        n_RandomThenMaxItem = 0;
                        maxSeriesMaxItemCount++;
                        findMax = true;
                    }
                    else
                    {
                        n_RandomThenMaxItem++;
                        maxSeriesMaxItemCount = 0;
                        findMax = false;
                    }

                    int index = findMax ? LevelUtils.FindMaxIndexInArray(leftItemCounts) :
                            LevelUtils.GetRandomIndexInArray(leftItemCounts);

                    itemColor = LevelData.ItemColorGroupDatas[index].ItemColor;
                    leftItemCounts[index]--;
                }
                else
                {
                    itemColor = colorGroupQueues.Peek();
                }

                var itemConfigData = ItemColorConfigDataManager.Instance.GetConfigData(itemColor);
                var item = Instantiate(itemConfigData.Item, transform);
                item.SetGeneration(generation);
                items[i] = item;

                spawnIndex--;

                if (spawnIndex % 3 == 0)
                {
                    colorGroupQueues.Dequeue();
                }
            }

            return items;
        }

        public float GetProgress()
        {
            return 1 - (float)itemCount / TotalItemCount;
        }

        public override void Dispose()
        {
            ColorGroupSloter.Dispose();
            base.Dispose();
        }
    }
}
