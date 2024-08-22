using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using SDKExtension;
using UnityEngine;

namespace GameExtension
{
    public abstract class GameDataManagerBase<T> : MonoBehaviour where T:GameDataBase
    {
        protected int maxBuffer = 30;
        protected float savingInternal = 10;

        protected T gameData;
        int buffer = 0;

        public void AddBuffer(int count = 1)
        {
            buffer += count;
            if (buffer >= maxBuffer)
            {
                SaveGameData();
            }
        }

        protected virtual string GetGameDataPath()
        {
            return Path.Combine(Application.persistentDataPath, "GameData");
        }

        protected abstract IEnumerator LoadGameData();

        public virtual void SaveGameData()
        {
            if (gameData == null)
            {
                return;
            }
            Internal_SaveGameData();
            gameData.LastSaveTime = new TimeRecord(DateTime.Now);
            var json = JsonConvert.SerializeObject(gameData,
                new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
            IOAdapter.Instance.Write(GetGameDataPath(), json);
            Logger.Log("保存游戏数据成功----");
            buffer = 0;
        }

        protected abstract void Internal_SaveGameData();

        public bool HasGameData()
        {
            return IOAdapter.Instance.IsExists(GetGameDataPath());
        }

        public void DeleteGameData()
        {
            var gameDataPath = GetGameDataPath();
            if (IOAdapter.Instance.IsExists(gameDataPath))
            {
                IOAdapter.Instance.Delete(gameDataPath);
            }
        }

        protected void TrySaveGameData_WhenBuffGreaterThanZero()
        {
            if (buffer >= maxBuffer)
            {
                SaveGameData();
            }
        }

        protected IEnumerator SavingGameData()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(savingInternal);
                if (buffer > 0)
                {
                    SaveGameData();
                }
            }
        }

        protected void OnApplicationQuit()
        {
            SaveGameData();
        }

        protected void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                SaveGameData();
            }
        }

        protected void UpdateDatasAfterLastSave()
        {
            if (gameData.LastSaveTime.Equals(default(TimeRecord)))
            {
                gameData.LastSaveTime = new TimeRecord(DateTime.Now);
            }
            var now = DateTime.Now;
            var lastSaveTime = gameData.LastSaveTime.ToDateTime();
            var elaspedSeconds = (float)(now - lastSaveTime).TotalMilliseconds * 0.001f;
            Internal_UpdateDatasAfterLastSave(lastSaveTime, now, elaspedSeconds);
        }

        protected abstract void Internal_UpdateDatasAfterLastSave(DateTime lastSaveTime, DateTime now, float elaspedSeconds);

        #region SavedData

        Dictionary<string, string> GetSavedData()
        {
            if (gameData.SavedData == null)
            {
                gameData.SavedData = new Dictionary<string, string>();
            }
            return gameData.SavedData;
        }

        public void SaveData<U>(string key, U obj)
        {
            string value = null;
            if (obj is string str)
            {
                value = str;
            }
            else
            {
                value = SerializeUtils.Serialize(obj);
            }
            GetSavedData()[key] = value;
            AddBuffer();
        }

        public void RemoveData(string key)
        {
            GetSavedData().Remove(key);
            AddBuffer();
        }

        public string LoadData(string key)
        {
            if (GetSavedData().TryGetValue(key, out string value))
            {
                return value;
            }
            return null;
        }
        public U LoadData<U>(string key, U defaultValue)
        {
            if (GetSavedData().TryGetValue(key, out string value))
            {
                return (U)SerializeUtils.Deserialize(value, typeof(U));
            }
            return defaultValue;
        }

        #endregion

        #region Date

        SignedHistoryData GetSignedHistoryData()
        {
            if (gameData.SignedHistoryData == null)
            {
                gameData.SignedHistoryData = new SignedHistoryData(new TimeRecord(DateTime.MinValue), default, default, default, default);
            }
            return gameData.SignedHistoryData;
        }

        public bool IsSigned()
        {
            return GetSignedHistoryData().IsSigned();
        }

        public void Sign()
        {
            GetSignedHistoryData().Sign();
            AddBuffer();
        }

        public bool IsNewDay()
        {
            return GetSignedHistoryData().IsNewDay();
        }

        public bool IsNewWeek()
        {
            return GetSignedHistoryData().IsNewWeek();
        }

        public bool IsNewMonth()
        {
            return GetSignedHistoryData().IsNewMonth();
        }

        public bool IsNewYear()
        {
            return GetSignedHistoryData().IsNewYear();
        }

        public int GetSignedDayInWeek()
        {
            return GetSignedHistoryData().SignedDayInWeek;
        }

        public int GetSignedDayInMonth()
        {
            return GetSignedHistoryData().SignedDayInMonth;
        }

        public int GetSignedDayInYear()
        {
            return GetSignedHistoryData().SignedDayInYear;
        }

        #endregion
    }

    [Serializable]
    public class GameDataBase
    {
        public TimeRecord LastSaveTime { get; set; }

        public SignedHistoryData SignedHistoryData { get; set; }

        public Dictionary<string, string> SavedData { get; set; }

        [JsonConstructor]
        public GameDataBase(TimeRecord lastExitTime, Dictionary<string, string> savedData, SignedHistoryData signedHistoryData)
        {
            LastSaveTime = lastExitTime;
            SavedData = savedData;
            SignedHistoryData = signedHistoryData;
        }
    }

    public class SignedHistoryData
    {
        //上一次签到日期
        public TimeRecord LastSignedDate { get; private set; }

        //总签到天数
        public int TotalSignedDay { get; private set; }

        //一周内签到天数
        public int SignedDayInWeek { get; private set; }

        //一个月内签到天数
        public int SignedDayInMonth { get; private set; }

        //一年内签到天数
        public int SignedDayInYear { get; private set; }

        [JsonConstructor]
        public SignedHistoryData(TimeRecord lastSignedDate, int totalSignedDay, int signedDayInWeek, int signedDayInMonth, int signedDayInYear)
        {
            LastSignedDate = lastSignedDate;
            TotalSignedDay = totalSignedDay;
            SignedDayInWeek = signedDayInWeek;
            SignedDayInMonth = signedDayInMonth;
            SignedDayInYear = signedDayInYear;
        }

        public bool IsNewDay()
        {
            var now = DateTime.Now;
            var lastDate = LastSignedDate.ToDateTime();
            return lastDate.Year != now.Year || lastDate.Month != now.Month || lastDate.Day != now.Day;
        }

        public bool IsNewWeek()
        {
            int lastWeek = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(LastSignedDate.ToDateTime(), CalendarWeekRule.FirstDay,
                DayOfWeek.Monday);
            int currentWeek = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay,
                DayOfWeek.Monday);
            return lastWeek != currentWeek;
        }

        public bool IsNewMonth()
        {
            return LastSignedDate.Months != DateTime.Now.Month;
        }

        public bool IsNewYear()
        {
            return LastSignedDate.Years != DateTime.Now.Year;
        }

        public bool IsSigned()
        {
            if (LastSignedDate.Equals(default(TimeRecord)))
            {
                LastSignedDate = new TimeRecord(DateTime.MinValue);
                TotalSignedDay = 0;
                SignedDayInWeek = 0;
                SignedDayInMonth = 0;
                SignedDayInYear = 0;
            }
            var now = DateTime.Now;
            return LastSignedDate.Days == now.Day && LastSignedDate.Months == now.Month && LastSignedDate.Years == now.Year;
        }

        public void Sign()
        {
            if (IsNewYear())
            {
                SignedDayInYear = 0;
            }
            if (IsNewMonth())
            {
                SignedDayInMonth = 0;
            }
            if (IsNewWeek())
            {
                SignedDayInWeek = 0;
            }
            LastSignedDate.Set(DateTime.Now);
            TotalSignedDay++;
            SignedDayInWeek++;
            SignedDayInMonth++;
            SignedDayInYear++;
        }
    }
}