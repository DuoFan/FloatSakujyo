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
        int[] leftItemCounts;
        public int SpawnIndex { get; private set; }

        public ColorGroupSloter ColorGroupSloter { get; private set; }

        int n_RandomThenMaxItem;
        int maxSeriesMaxItemCount;

        public override IEnumerator Init(SubLevelData levelData,ColorGroupSloter colorGroupSloter)
        {
            SubLevelData = levelData;

            Array.Sort(levelData.LevelDifficultyDatas, (x, y) => x.LeftItemCount.CompareTo(y.LeftItemCount));

            ColorGroupSloter = colorGroupSloter; 

            leftItemCounts = LevelUtils.GetColorGroupCounts(levelData);
            for (int i = 0; i < leftItemCounts.Length; i++)
            {
                leftItemCounts[i] *= 3;
            }

            SpawnIndex = SubLevelData.TotalItemCount;
            n_RandomThenMaxItem = 0;
            maxSeriesMaxItemCount = 0;

            yield break;
        }

        public Item[] SpawnItems(int spawnCount, ItemGeneration generation)
        {
            var items = new Item[spawnCount];
            for (int i = 0; i < spawnCount && SpawnIndex > 0; i++)
            {
                LevelDifficultyData curDifficultyData = null;
                int index;
                for (int j = 0; j < SubLevelData.LevelDifficultyDatas.Length; j++)
                {
                    if (SpawnIndex <= SubLevelData.LevelDifficultyDatas[j].LeftItemCount)
                    {
                        curDifficultyData = SubLevelData.LevelDifficultyDatas[j];
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

                    index = findMax ? LevelUtils.FindMaxIndexInArray(leftItemCounts) :
                            LevelUtils.GetRandomIndexInArray(leftItemCounts);
                }
                else
                {
                    index = LevelUtils.FindMaxIndexInArray(leftItemCounts);
                }

                var itemColor = SubLevelData.ItemColorGroupDatas[index].ItemColor;

                items[i] = SpawnItemByItemColor(itemColor, generation);
            }

            return items;
        }

        public Item SpawnItemByItemColor(ItemColor itemColor,ItemGeneration generation)
        {
            var index = Array.IndexOf(SubLevelData.ItemColorGroupDatas, SubLevelData.ItemColorGroupDatas.FirstOrDefault(x => x.ItemColor == itemColor));
            leftItemCounts[index]--;

            var itemConfigData = ItemColorConfigDataManager.Instance.GetConfigData(itemColor);
            var item = Instantiate(itemConfigData.Item, transform);
            item.SetGeneration(generation);

            SpawnIndex--;

            return item;
        }
    }
}
