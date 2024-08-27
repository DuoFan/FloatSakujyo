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
        public int LevelID { get; set; }
        public TutorialHistoryData TutorialHistoryData { get; set; }

        public PlayerPreference PlayerPreference { get; set; }  

        public ShareHistoryData ShareHistoryData { get; set; }

        [JsonConstructor]
        public GameData(TimeRecord lastExitTime, Dictionary<string, string> savedData,
            SignedHistoryData signedHistoryData, int levelID, bool isFirstGame, TutorialHistoryData tutorialHistoryData,
            PlayerPreference playerPreference, ShareHistoryData shareHistoryData) : base(lastExitTime, savedData, signedHistoryData)
        {
            LevelID = levelID;
            IsFirstGame = isFirstGame;
            TutorialHistoryData = tutorialHistoryData;
            PlayerPreference = playerPreference;
            ShareHistoryData = shareHistoryData;
        }
    }
}

