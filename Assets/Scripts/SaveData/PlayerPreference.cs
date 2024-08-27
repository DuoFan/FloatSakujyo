using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloatSakujyo.SaveData
{
    public class PlayerPreference
    {
        public bool IsMute { get; set; }
        public bool HasVibration { get; set; }

        [JsonConstructor]
        public PlayerPreference(bool isMute, bool hasVibration)
        {
            IsMute = isMute;
            HasVibration = hasVibration;
        }
    }
}

