using EditorExtension;
using FloatSakujyo.Level;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorExtension
{
    public static partial class EditorUtils
    {
        [MenuItem("EditorExtension/Tools/SetAddressToLevelData", false, 0)]
        private static void SetAddressToLevelData()
        {
            foreach (UnityEngine.Object obj in Selection.objects)
            {
                var levelData = obj as LevelData;
                if (levelData == null)
                {
                    continue;
                }
                var entry = EditorUtils.GetAddressableAssetEntry(levelData);
                if (entry == null)
                {
                    entry = EditorUtils.CreateEntryToGroupByGroupName(levelData, "LevelData");
                }
                entry.address = "LevelData";
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

