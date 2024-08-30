using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloatSakujyo.SaveData
{
    public class LevelHistoryData
    {
        public int LevelID { get; set; }
        public int SubLevelID { get; set; }

        [JsonConstructor]
        public LevelHistoryData(int levelID, int subLevelID)
        {
            LevelID = levelID;
            SubLevelID = subLevelID;
        }
    }
}

