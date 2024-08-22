using GameExtension;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameExtension
{
    public abstract class KeyDataManagerBase<TKey, TConfig> : SingletonMonoBase<KeyDataManagerBase<TKey, TConfig>>
    {
        [SerializeField]
        private TConfig[] configDatas;

        private Dictionary<TKey, TConfig> configDataDict;

        protected override void Awake()
        {
            base.Awake();
            if (Instance == this)
            {
                Init();
            }
        }

        void Init()
        {
            configDataDict = new Dictionary<TKey, TConfig>();
            foreach (var configData in configDatas)
            {
                var key = GetKeyFromConfig(configData);
                configDataDict[key] = configData;
            }
            configDatas = null;
        }

        protected abstract TKey GetKeyFromConfig(TConfig configData);

        public TConfig GetConfigData(TKey key)
        {
            return configDataDict[key];
        }

        public bool TryGetConfigData(TKey key, out TConfig configData)
        {
            return configDataDict.TryGetValue(key, out configData);
        }

        public TConfig[] GetConfigDatas()
        {
            return configDataDict.Values.ToArray();
        }
    }
}
