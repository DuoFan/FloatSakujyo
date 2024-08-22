using FloatSakujyo.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloatSakujyo.Level
{
    [System.Serializable]
    public class LevelItemColorGroupData
    {
        [SerializeField]
        ItemColor itemColor;
        public ItemColor ItemColor => itemColor;

        [SerializeField]
        int groupCount;
        public int GroupCount => groupCount;

        public LevelItemColorGroupData(ItemColor itemColor, int groupCount)
        {
            this.itemColor = itemColor;
            this.groupCount = groupCount;
        }
    }
}
