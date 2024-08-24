using System;

using UnityEngine;

namespace FloatSakujyo.Level
{
    [Serializable]
    public class LevelDifficultyData
    {
        [SerializeField]
        int leftGroupCount;
        public int LeftGroupCount => leftGroupCount;

        [SerializeField]
        int maxRandomEasyValue;
        public int MaxRandomEasyValue => maxRandomEasyValue;

        [SerializeField]
        int n_HardThenEasy;
        public int N_HardThenEasy => n_HardThenEasy;

        [SerializeField]
        int maxEasyGroupCount;
        public int MaxEasyGroupCount => maxEasyGroupCount;

        public LevelDifficultyData(int leftGroupCount, int maxRandomValue,int n_HardThenEasy, int maxEasyGroupCount)
        {
            this.leftGroupCount = leftGroupCount;
            this.maxRandomEasyValue = maxRandomValue;
            this.n_HardThenEasy = n_HardThenEasy;
            this.maxEasyGroupCount = maxEasyGroupCount;
        }
    }
}
