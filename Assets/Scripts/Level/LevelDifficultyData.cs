using System;

using UnityEngine;

namespace FloatSakujyo.Level
{
    [Serializable]
    public class LevelDifficultyData
    {
        [SerializeField]
        int leftItemCount;
        public int LeftItemCount => leftItemCount;

        [SerializeField]
        int maxGroupRandomValue;
        public int MaxGroupRandomValue => maxGroupRandomValue;

        [SerializeField]
        int n_MinThenMaxGroup;
        public int N_MinThenMaxGroup => n_MinThenMaxGroup;

        [SerializeField]
        int maxSeriesMaxGroupCount;
        public int MaxSeriesMaxGroupCount => maxSeriesMaxGroupCount;

        [SerializeField]
        int maxItemRandomValue;
        public int MaxItemRandomValue => maxItemRandomValue;
        [SerializeField]
        int n_RandomThenMaxItem;
        public int N_RandomThenMaxItem => n_RandomThenMaxItem;
        [SerializeField]
        int maxSeriesMaxItemCount;
        public int MaxSeriesMaxItemCount => maxSeriesMaxItemCount;

        public LevelDifficultyData(int leftItemCount, int maxGroupRandomValue, int n_MinThenMaxGroup, int maxSeriesMaxGroupCount,
            int maxItemRandomValue, int n_RandomThenMaxItem, int maxSeriesMaxItemCount)
        {
            this.leftItemCount = leftItemCount;
            this.maxGroupRandomValue = maxGroupRandomValue;
            this.n_MinThenMaxGroup = n_MinThenMaxGroup;
            this.maxSeriesMaxGroupCount = maxSeriesMaxGroupCount;
            this.maxItemRandomValue = maxItemRandomValue;
            this.n_RandomThenMaxItem = n_RandomThenMaxItem;
            this.maxSeriesMaxItemCount = maxSeriesMaxItemCount;
        }
    }
}
