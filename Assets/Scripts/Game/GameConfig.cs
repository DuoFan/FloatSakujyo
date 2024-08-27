using GameExtension;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloatSakujyo
{
    public class GameConfig : SingletonMonoBase<GameConfig>
    {
        [SerializeField]
        private int completedLevelCoin;
        public int CompletedLevelCoin => completedLevelCoin;
        [SerializeField]
        private int completedLevelCoinMultiple;
        public int CompletedLevelCoinMultiple => completedLevelCoinMultiple;
    }
}

