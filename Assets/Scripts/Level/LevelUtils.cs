using GameExtension;
using FloatSakujyo.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
using UnityEngine.EventSystems;
using System.Linq;
using WeChatWASM;

namespace FloatSakujyo.Level
{
    public static class LevelUtils
    {
        public static AsyncGetHandle<LevelEntity> CreateLevelEntity(SubLevelData levelData, ColorGroupSloter colorGroupSloter)
        {
            var handle = new AsyncGetHandle<LevelEntity>();
            var levelEntity = new GameObject($"LevelEntity").AddComponent<LevelEntity>();

            CoroutineManager.Instance.StartCoroutine(WaitForInitLevel());

            IEnumerator WaitForInitLevel()
            {
                yield return levelEntity.Init(levelData, colorGroupSloter);
                handle.SetResult(levelEntity);
            }

            return handle;
        }


        public static List<ItemColor> GenerateColorGroupQueue(SubLevelData levelData)
        {
            List<ItemColor> colorGroupQueues = null;
            if (levelData.IsCustomColorGroup && levelData.ItemColorGroupDatas.Length > 0)
            {
                colorGroupQueues = new List<ItemColor>(levelData.CustomColorGroupQueue);
            }
            else
            {
                colorGroupQueues = new List<ItemColor>();

                int[] leftColorGroupCounts = GetColorGroupCounts(levelData);
                var itemCount = leftColorGroupCounts.Sum(x => x * 3);

                var levelDifficultyDatas = levelData.LevelDifficultyDatas;

                int n_MinGroup = 0;
                int maxSeriesMaxGroupCount = 0;

                while (itemCount > 0)
                {
                    LevelDifficultyData curDifficultyData = null;

                    for (int i = 0; i < levelDifficultyDatas.Length; i++)
                    {
                        if (itemCount <= levelDifficultyDatas[i].LeftItemCount)
                        {
                            curDifficultyData = levelData.LevelDifficultyDatas[i];
                            break;
                        }
                    }

                    if (curDifficultyData != null)
                    {
                        bool findMax;
                        var randomValue = UnityEngine.Random.Range(0, 100);
                        if ((randomValue < curDifficultyData.MaxGroupRandomValue || n_MinGroup >= curDifficultyData.N_MinThenMaxGroup) &&
                            maxSeriesMaxGroupCount < curDifficultyData.MaxSeriesMaxGroupCount)
                        {
                            n_MinGroup = 0;
                            maxSeriesMaxGroupCount++;
                            findMax = true;
                        }
                        else
                        {
                            n_MinGroup++;
                            maxSeriesMaxGroupCount = 0;
                            findMax = false;
                        }

                        int index = findMax ? FindMaxIndexInArray(leftColorGroupCounts) : FindMinIndexInArray(leftColorGroupCounts);
                        colorGroupQueues.Add(levelData.ItemColorGroupDatas[index].ItemColor);
                        leftColorGroupCounts[index]--;
                    }
                    else
                    {
                        var index = GetRandomIndexInArray(leftColorGroupCounts);

                        colorGroupQueues.Add(levelData.ItemColorGroupDatas[index].ItemColor);
                        leftColorGroupCounts[index]--;
                    }

                    itemCount -= 3;
                }
            }
            return colorGroupQueues;
        }

        public static int[] GetColorGroupCounts(SubLevelData levelData)
        {
            int[] colorGroupCounts = new int[levelData.ItemColorGroupDatas.Length];
            for (int i = 0; i < levelData.ItemColorGroupDatas.Length; i++)
            {
                colorGroupCounts[i] = levelData.ItemColorGroupDatas[i].GroupCount;
            }
            return colorGroupCounts;
        }

        public static int FindMaxIndexInArray(int[] array)
        {
            int value = 0;
            int index = -1;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] > value)
                {
                    value = array[i];
                    index = i;
                }
            }

            return index;
        }

        public static int FindMinIndexInArray(int[] array)
        {
            int value = int.MaxValue;
            int index = -1;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] > 0 && array[i] < value)
                {
                    value = array[i];
                    index = i;
                }
            }

            return index;
        }

        public static int GetRandomIndexInArray(int[] array)
        {
            var total = array.Sum(x => x);
            var value = UnityEngine.Random.Range(0, total);
            int index = -1;
            while (value >= 0)
            {
                index++;
                value -= array[index];
                if (value < 0)
                {
                    break;
                }
            }
            return index;
        }

        public static int RoundToInt(float value)
        {
            return (value >= 0) ? Mathf.FloorToInt(value + 0.5f) : Mathf.CeilToInt(value - 0.5f);
        }
    }
}
