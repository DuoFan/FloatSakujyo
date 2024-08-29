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
        SubLevelData[] subLevelDatas;
        public SubLevelData[] SubLevelDatas => subLevelDatas;

        public void Init(int id, SubLevelData[] subLevelDatas)
        {
            this.id = id;
            this.subLevelDatas = subLevelDatas;
        }
    }
}
