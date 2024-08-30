using GameExtension;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace FloatSakujyo.SaveData
{
    public class GameData : GameDataBase
    {
        public bool IsFirstGame { get; set; }
        public LevelHistoryData LevelHistoryData { get; set; }
        public TutorialHistoryData TutorialHistoryData { get; set; }

        public PlayerPreference PlayerPreference { get; set; }  

        public ShareHistoryData ShareHistoryData { get; set; }

        public HelperCountData HelperCountData { get; set; }

        [JsonConstructor]
        public GameData(TimeRecord lastExitTime, Dictionary<string, string> savedData,
            SignedHistoryData signedHistoryData, LevelHistoryData levelHistoryData, bool isFirstGame, TutorialHistoryData tutorialHistoryData,
            PlayerPreference playerPreference, ShareHistoryData shareHistoryData, HelperCountData helperCountData) : base(lastExitTime, savedData, signedHistoryData)
        {
            LevelHistoryData = levelHistoryData;
            IsFirstGame = isFirstGame;
            TutorialHistoryData = tutorialHistoryData;
            PlayerPreference = playerPreference;
            ShareHistoryData = shareHistoryData;
            HelperCountData = helperCountData;
        }
    }
}

