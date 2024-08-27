using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloatSakujyo.SaveData
{
    public class ShareHistoryData
    {
        public bool IsSharedForRestore { get; private set; }
        [JsonConstructor]
        public ShareHistoryData(bool isSharedForRestore)
        {
            IsSharedForRestore = isSharedForRestore;
        }
        public void SetIsSharedForRestore(bool isSharedForRestore)
        {
            IsSharedForRestore = isSharedForRestore;
        }
    }
}

