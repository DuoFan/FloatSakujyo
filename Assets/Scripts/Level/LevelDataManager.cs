using GameExtension;
using FloatSakujyo.Level;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FloatSakujyo.Game
{
    public class LevelDataManager : SingletonMonoBase<LevelDataManager>,IGameInitializer
    {
        protected Dictionary<int, LevelData> datas;

        public IEnumerator InitializeGame()
        {
            var handle = AddressableManager.Instance.LoadAssetsAsync<LevelData>("LevelData");
            yield return handle.WaitForCompletion();

            datas = new Dictionary<int, LevelData>();

            if(handle.Result != null)
            {
                foreach(var data in handle.Result)
                {
                    datas.Add(data.ID, data);
                }
            }

            yield break;
        }

        public LevelData GetData(int id)
        {
            return datas[id];
        }
        public LevelData[] GetDatas()
        {
            return datas.Values.ToArray();
        }

        public bool TryGetData(int id, out LevelData data)
        {
            return datas.TryGetValue(id, out data);
        }

        public LevelData GetNextLevel(LevelData prevLevel)
        {
            var keys = datas.Keys.ToArray();
            Array.Sort(keys);

            if (prevLevel == null)
            {
                return datas[keys[0]];
            }

            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] == prevLevel.ID)
                {
                    if (i + 1 < keys.Length)
                    {
                        return datas[keys[i + 1]];
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        public LevelData GetFirstLevel()
        {
            var keys = datas.Keys.ToArray();
            Array.Sort(keys);

            return datas[keys[0]];
        }
    }
}
