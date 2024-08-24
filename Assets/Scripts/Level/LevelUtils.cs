using GameExtension;
using FloatSakujyo.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
using UnityEngine.EventSystems;

namespace FloatSakujyo.Level
{
    public static class LevelUtils
    {
        public static AsyncGetHandle<LevelEntity> CreateLevelEntity(LevelData levelData)
        {
            var handle = new AsyncGetHandle<LevelEntity>();
            var levelEntity = new GameObject($"LevelEntity{levelData.ID}").AddComponent<LevelEntity>();

            CoroutineManager.Instance.StartCoroutine(WaitForInitLevel());

            IEnumerator WaitForInitLevel()
            {
                yield return levelEntity.Init(levelData);
                handle.SetResult(levelEntity);
            }

            return handle;
        }

        public static ColorGroupSloter GenerateColorQueueSloter(LevelData levelData, int defaultNoneGroupSlotCount, bool isGroupDefalutUseable,out List<ItemColor> colorGroupQueues)
        {
            if (levelData.IsCustomColorGroup && levelData.ItemColorGroupDatas.Length > 0)
            {
                colorGroupQueues = new List<ItemColor>(levelData.CustomColorGroupQueue);
            }
            else
            {
                int colorGroupCount = 0;
                int[] leftColorGroupCounts = new int[levelData.ItemColorGroupDatas.Length];
                for (int i = 0; i < levelData.ItemColorGroupDatas.Length; i++)
                {
                    colorGroupCount += levelData.ItemColorGroupDatas[i].GroupCount;
                    leftColorGroupCounts[i] = levelData.ItemColorGroupDatas[i].GroupCount;
                }

                colorGroupQueues = new List<ItemColor>(colorGroupCount);
                var random = new System.Random();
                while (colorGroupCount > 0)
                {
                    var index = random.Next(0, colorGroupCount); ;
                    int j = -1;
                    while (index >= 0)
                    {
                        j++;
                        index -= leftColorGroupCounts[j];
                        if (index < 0)
                        {
                            break;
                        }
                    }

                    colorGroupQueues.Add(levelData.ItemColorGroupDatas[j].ItemColor);
                    leftColorGroupCounts[j]--;
                    colorGroupCount--;
                }
            }
            return new ColorGroupSloter(new List<ItemColor>(colorGroupQueues), defaultNoneGroupSlotCount, isGroupDefalutUseable);
        }

        public static int RoundToInt(float value)
        {
            return (value >= 0) ? Mathf.FloorToInt(value + 0.5f) : Mathf.CeilToInt(value - 0.5f);
        }
    }
}
