using GameExtension;
using Newtonsoft.Json;
using FloatSakujyo.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloatSakujyo.Level
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "FloatSakujyo/LevelData")]
    public class LevelData : ScriptableObject, IConfigData, IScrollListData
    {
        [SerializeField]
        int id;
        public int ID => id;
        public string DataName => $"关卡{ID}";

        [SerializeField]
        LevelItemColorGroupData[] itemColorGroupDatas;
        public LevelItemColorGroupData[] ItemColorGroupDatas => itemColorGroupDatas;

        [SerializeField]
        bool isCustomColorGroup;
        public bool IsCustomColorGroup => isCustomColorGroup;

        [SerializeField]
        ItemColor[] customColorGroupQueue;
        public ItemColor[] CustomColorGroupQueue => customColorGroupQueue;

        [SerializeField, Header("新生代,沉底的物品数量")]
        int edenItemCount = 5;
        public int EdenItemCount => edenItemCount;

        [SerializeField, Header("中间代,中部的物品数量")]
        int midItemCount = 5;
        public int MidItemCount => midItemCount;

        [SerializeField, Header("老年代,漂浮的物品数量")]
        int oldItemCount = 10;
        public int OldItemCount => oldItemCount;

        public void Init(int id, LevelItemColorGroupData[] screwColorGroupDatas, bool isCustomColorGroup, ItemColor[] customColorGroupQueue)
        {
            this.id = id;

            this.itemColorGroupDatas = screwColorGroupDatas;
            this.isCustomColorGroup = isCustomColorGroup;
            this.customColorGroupQueue = customColorGroupQueue;
        }
    }
}
