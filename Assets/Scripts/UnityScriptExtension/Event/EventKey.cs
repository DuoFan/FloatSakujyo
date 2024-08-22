using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public static partial class EventKey
    {
        public const string OnTouchDown = "OnTouchDown";
        public const string OnTouchUp = "OnTouchUp";
        public const string OnTouching = "OnTouching";

        public const string OnPlayerRespone = "OnPlayerRespone";

        public const string OnStartTask = "OnStartTask";
        public const string OnCompleteTask = "OnCompleteTask";
        public const string OnHintCompleteRecurringTask = "OnHintCompleteRecurringTask";

        public const string OnUnlockFunction = "OnUnlockFunction";

        public static string OnResetDailyUseData = "OnResetDailyUseData";

        public const string OnAutoGetTaskReward = "OnAutoGetTaskReward";
        public const string HintManualGetTaskReward = "OnManualGetTaskReward";
    }
}