using GameExtension;
using Newtonsoft.Json;
using FloatSakujyo.Level;
using FloatSakujyo.Tutorial;
using SDKExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;
using Logger = GameExtension.Logger;

namespace FloatSakujyo.SaveData
{
    public class GameDataManager : GameDataManagerBase<GameData>, IGameInitializer, ITutorialDataManager
    {
        public static GameDataManager Instance => instance;
        static GameDataManager instance;
        private void Awake()
        {
            Singleton.KeepWhenSingletonNull(ref instance, this);
        }

        protected override void Internal_SaveGameData()
        {

        }

        protected override IEnumerator LoadGameData()
        {
            if (HasGameData())
            {
                GameExtension.Logger.Log("加载游戏数据---");
                yield return IOAdapter.Instance.ReadText(GetGameDataPath(), (json) =>
                {
                    gameData = JsonConvert.DeserializeObject<GameData>(json,
                        new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                    GameExtension.Logger.Log("加载游戏数据成功---");
                });
                GameExtension.Logger.Log("加载游戏数据成功---");
            }
            else
            {
                GameExtension.Logger.Log("初始化游戏数据----");
                //关卡从1开始
                gameData = new GameData(default, default, default, default, true, default, default, default, default);
                GameExtension.Logger.Log("初始化游戏数据成功----");
            }
            yield break;
        }

        public IEnumerator InitializeGame()
        {
            TutorialControllerBase.SetTutorialDataManager(this);

            yield return LoadGameData();
            UpdateDatasAfterLastSave();
            StartCoroutine(SavingGameData());
            yield break;
        }

        protected override void Internal_UpdateDatasAfterLastSave(DateTime lastSaveTime, DateTime now, float elaspedSeconds)
        {

        }

        public bool IsFirstGame()
        {
            return gameData.IsFirstGame;
        }

        public void FlagNotFirstGame()
        {
            gameData.IsFirstGame = false;
        }

        #region Level

        public LevelHistoryData GetLevelHistoryData()
        {
            if (gameData.LevelHistoryData == null)
            {
                gameData.LevelHistoryData = new LevelHistoryData(1, 0);
            }
            return gameData.LevelHistoryData;
        }

        public int GetLevelID()
        {
            return GetLevelHistoryData().LevelID;
        }

        public void SetLevelID(int levelID)
        {
            GetLevelHistoryData().LevelID = levelID;
            GetLevelHistoryData().SubLevelID = 0;
            AddBuffer();
        }

        public int GetSubLevelID()
        {
            return GetLevelHistoryData().SubLevelID;
        }
        public void SetSubLevelID(int subLevelID)
        {
            GetLevelHistoryData().SubLevelID = subLevelID;
            AddBuffer();
        }
        #endregion

        #region Tutorial
        public TutorialHistoryData GetTutorialHistoryData()
        {
            if (gameData.TutorialHistoryData == null)
            {
                gameData.TutorialHistoryData = new TutorialHistoryData(null);
            }

            if (gameData.TutorialHistoryData.TutorialStepHistory == null)
            {
                gameData.TutorialHistoryData.TutorialStepHistory = new Dictionary<string, int>();
            }
            return gameData.TutorialHistoryData;
        }

        #endregion

        #region PlayerPreference

        public PlayerPreference GetPlayerPreference()
        {
            if (gameData.PlayerPreference == null)
            {
                gameData.PlayerPreference = new PlayerPreference(false, true);
            }
            return gameData.PlayerPreference;
        }

        #endregion

        #region Share

        ShareHistoryData GetShareHistoryData()
        {
            if (gameData.ShareHistoryData == null)
            {
                gameData.ShareHistoryData = new ShareHistoryData(false);
            }
            return gameData.ShareHistoryData;
        }

        public bool IsSharedForRestore()
        {
            return GetShareHistoryData().IsSharedForRestore;
        }

        public void FlagSharedForRestore()
        {
            GetShareHistoryData().SetIsSharedForRestore(true);
            AddBuffer();
        }

        #endregion

        #region Helper

        HelperCountData GetHelperCountData()
        {
            if(gameData.HelperCountData == null)
            {
                gameData.HelperCountData = new HelperCountData(new Dictionary<HelperType, int>());
            }
            return gameData.HelperCountData;
        }

        public int GetHelperCount(HelperType helperType)
        {
            GetHelperCountData().HelperCount.TryGetValue(helperType, out var count);
            return count;
        }

        public void AddHelperCount(HelperType helperType,int count)
        {
            if(GetHelperCountData().HelperCount.TryGetValue(helperType, out var currentCount))
            {
                GetHelperCountData().HelperCount[helperType] = currentCount + count;
            }
            else
            {
                GetHelperCountData().HelperCount.Add(helperType, count);
            }
            AddBuffer();
        }

        public void SubHelperCount(HelperType helperType)
        {
            if(GetHelperCountData().HelperCount.TryGetValue(helperType, out var count) && count > 0)
            {
                GetHelperCountData().HelperCount[helperType] = count - 1;
            }
            else
            {
                GetHelperCountData().HelperCount.Add(helperType, 0);
            }

            AddBuffer();
        }

        #endregion

        protected override string GetGameDataPath()
        {
#if WEI_XIN
            return "GameData";
#else
            return base.GetGameDataPath();
#endif
        }
    }

}