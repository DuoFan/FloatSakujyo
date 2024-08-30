using GameExtension;
using Newtonsoft.Json;
using FloatSakujyo.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloatSakujyo.Level
{
    [System.Serializable]
    public class SubLevelData
    {
        [SerializeField]
        LevelItemColorGroupData[] itemColorGroupDatas;
        public LevelItemColorGroupData[] ItemColorGroupDatas => itemColorGroupDatas;

        [SerializeField, HideInInspector]
        int totalItemCount;
        public int TotalItemCount => totalItemCount;

        [SerializeField]
        bool isCustomColorGroup;
        public bool IsCustomColorGroup => isCustomColorGroup;

        [SerializeField]
        ItemColor[] customColorGroupQueue;
        public ItemColor[] CustomColorGroupQueue => customColorGroupQueue;

        [SerializeField]
        LevelDifficultyData[] levelDifficultyDatas;
        public LevelDifficultyData[] LevelDifficultyDatas => levelDifficultyDatas;


        [SerializeField, Header("漂浮的物品数量")]
        int floatItemCount = 10;
        public int FloatItemCount => floatItemCount;

        [SerializeField, Header("解锁物品ID")]
        int[] unlockItemIDs;
        public int[] UnlockItemIDs => unlockItemIDs;

        public SubLevelData(LevelItemColorGroupData[] itemColorGroupDatas, bool isCustomColorGroup, ItemColor[] customColorGroupQueue,
            LevelDifficultyData[] levelDifficultyDatas, int floatItemCount)
        {
            this.itemColorGroupDatas = itemColorGroupDatas;
            this.isCustomColorGroup = isCustomColorGroup;
            this.customColorGroupQueue = customColorGroupQueue;
            this.levelDifficultyDatas = levelDifficultyDatas;
            this.floatItemCount = floatItemCount;

            totalItemCount = 0;
            for (int i = 0; i < itemColorGroupDatas.Length; i++)
            {
                totalItemCount += itemColorGroupDatas[i].GroupCount * 3;
            }
        }
    }
}
