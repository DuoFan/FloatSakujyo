using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class TutorialHistoryData
    {
        public Dictionary<string, int> TutorialStepHistory;

        [JsonConstructor]
        public TutorialHistoryData(Dictionary<string, int> tutorialHistoryData)
        {
            TutorialStepHistory = tutorialHistoryData;
        }
    }
}
