using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FloatSakujyo.Level;
using Newtonsoft.Json;
using Level;
using System;
using WeChatWASM;
using static log4net.Appender.ColoredConsoleAppender;
using System.Linq;
using System.Text;

namespace FloatSakujyo.Editor
{
    [CustomEditor(typeof(RefLevelDataManager))]
    public class CustomRefLevelDataManagerEditor : UnityEditor.Editor
    {
        [SerializeField]
        TextAsset levelJson;

        [SerializeField]
        int maxFloatItemCount = 18;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            levelJson = EditorGUILayout.ObjectField("参考关卡Json", levelJson, typeof(TextAsset), false) as TextAsset;

            maxFloatItemCount = EditorGUILayout.IntField("最大漂浮物品数量", maxFloatItemCount);

            if (levelJson != null && maxFloatItemCount > 0 && GUILayout.Button("生成关卡文件"))
            {
                CreateLevelDatas();
            }
        }

        void CreateLevelDatas()
        {
            AssetDatabase.StartAssetEditing();
            var folderPath = $"LevelDatas_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}";
            AssetDatabase.CreateFolder("Assets", folderPath);
            folderPath = $"Assets/{folderPath}";
            var refLevelDatas = JsonConvert.DeserializeObject<RefLevelData[]>(levelJson.text);

            int maxItemColorCount = 0;
            int levelID = 1;
            HashSet<int> itemIDs = new HashSet<int>();
            for (int i = 0; i < refLevelDatas.Length; i++)
            {
                var floatItemCount = refLevelDatas[i].data.Sum(x => x > 0 ? 1 : 0);

                if (floatItemCount > maxFloatItemCount)
                {
                    continue;
                }

                var levelData = ScriptableObject.CreateInstance<LevelData>();

                LevelItemColorGroupData[] levelItemColorGroupDatas = new LevelItemColorGroupData[refLevelDatas[i].itemId.Length];
                var totalItemGroupCount = refLevelDatas[i].maxNum / 3;
                var perGroupItemCount = totalItemGroupCount / (float)refLevelDatas[i].itemId.Length;
                float count = 0;
                for (int j = 0; j < levelItemColorGroupDatas.Length; j++)
                {
                    count += perGroupItemCount;
                    if (j < levelItemColorGroupDatas.Length - 1)
                    {
                        levelItemColorGroupDatas[j] = new LevelItemColorGroupData(Game.ItemColor.Color1 + j, (int)count);
                    }
                    else
                    {
                        levelItemColorGroupDatas[j] = new LevelItemColorGroupData(Game.ItemColor.Color1 + j, totalItemGroupCount);
                    }
                    totalItemGroupCount -= (int)count;
                    count %= 1;
                }

                LevelDifficultyData[] levelDifficultyDatas = new LevelDifficultyData[refLevelDatas[i].easyOrder.Length];
                for (int j = 0; j < levelDifficultyDatas.Length; j++)
                {
                    int leftItemCount = refLevelDatas[i].easyOrder[j][0];
                    int maxGroupRandomValue = refLevelDatas[i].easyOrder[j][1];
                    int n_MinThenMaxGroup = refLevelDatas[i].easyInsurance[j][1];
                    int maxSeriesMaxGroupCount = refLevelDatas[i].easyInsurance2[j][1];

                    int maxItemRandomValue = refLevelDatas[i].easyOrder[j][1];
                    int n_RandomThenMaxItem = refLevelDatas[i].easyInsurance[j][1];
                    int maxSeriesMaxItemCount = refLevelDatas[i].easyInsurance2[j][1];

                    LevelDifficultyData levelDifficultyData = new LevelDifficultyData(leftItemCount, maxGroupRandomValue, n_MinThenMaxGroup,
                        maxSeriesMaxGroupCount, maxItemRandomValue, n_RandomThenMaxItem, maxSeriesMaxItemCount);
                    levelDifficultyDatas[j] = levelDifficultyData;
                }

                for (int j = 0; j < refLevelDatas[i].itemId.Length; j++)
                {
                    itemIDs.Add(refLevelDatas[i].itemId[j]);
                }

                levelData.Init(levelID, levelItemColorGroupDatas, false, null, levelDifficultyDatas, floatItemCount);

                maxItemColorCount = Mathf.Max(maxItemColorCount, levelItemColorGroupDatas.Length);

                AssetDatabase.CreateAsset(levelData, $"{folderPath}/LevelData_{refLevelDatas[i].id}.asset");

                levelID++;
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("最大物品颜色数量：" + maxItemColorCount);

            var itemIDArray = itemIDs.ToArray();
            Array.Sort(itemIDArray);
            stringBuilder.AppendLine("物品ID：" + string.Join(",", itemIDArray));

            Debug.Log(stringBuilder.ToString());

            AssetDatabase.StopAssetEditing();
        }
    }
}

