using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloatSakujyo.SaveData
{
    public class HelperCountData 
    {
        public Dictionary<HelperType, int> HelperCount { get; private set; }

        [JsonConstructor]
        public HelperCountData(Dictionary<HelperType, int> helperCount)
        {
            HelperCount = helperCount;
        }
        
    }
}

