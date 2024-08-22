using GameExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloatSakujyo.Game
{
    public class ItemColorConfigDataManager : KeyDataManagerBase<ItemColor, ItemColorConfigData>
    {
        protected override ItemColor GetKeyFromConfig(ItemColorConfigData configData)
        {
            return configData.ItemColor;
        }

    }
}
