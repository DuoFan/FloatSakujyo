using Proyecto26;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    [System.Serializable]
    public class RemoteGameConfig
    {
        public static RemoteGameConfig Instance { get; private set; }
        public bool isOpenDebug;
        static string url;
        static Action<RemoteGameConfig> onLoaded;
        static Action<Exception> onLoadFail;
        public static void LoadConfig(string _url,Action<RemoteGameConfig> _onLoaded = null, Action<Exception> _onLoadFail = null)
        {
            onLoaded = _onLoaded;
            onLoadFail = _onLoadFail;
            url = _url;
            RestClient.Get<RemoteGameConfig>($"{url}?j={DateTime.Now.ToUniversalTime().Ticks}").Then(OnLoaded, OnLoadFail);
        }
        static void OnLoaded(RemoteGameConfig config)
        {
            Instance = config;
            onLoaded?.Invoke(config);
            GameExtension.Logger.Log($"加载远程配置成功:url:{url}");
        }
        static void OnLoadFail(Exception exception)
        {
            onLoadFail?.Invoke(exception);
            GameExtension.Logger.Log($"加载远程配置失败:url:{url}");
        }
    }
}
