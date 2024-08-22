using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SDKExtension
{
    public interface ISDK
    {
        bool IsTest { get; set; }
        void Init();
        Action OnInited { get; set; }
        void Test();
    }

    public class SDKListener : MonoBehaviour
    {
        public static SDKListener Instance { get; private set; }
        private static void CheckInstance()
        {
            if (Instance == null && Application.isPlaying)
            {
                var go = new GameObject(
                    "SDKListener", typeof(SDKListener));
                GameObject.DontDestroyOnLoad(go);
                Instance = go.GetComponent<SDKListener>();
            }
        }
        public static void Init(IADPlayer player, Action OnInited)
        {
            if (Instance == null)
            {
                CheckInstance();
#if USE_SDK
                Instance.InitAD(player, OnInited);
#else
                Instance.InitAD(new StubAD(), OnInited);
#endif
            }
        }

        #region AD

        public Dictionary<string, IAD> ExtraAD
        {
            get; private set;
        }

        public void AddExtraAD(string tag,IAD ad)
        {
            if(ExtraAD == null)
            {
                ExtraAD = new Dictionary<string, IAD>();    
            }
            ExtraAD[tag] = ad;
        }

        public T GetExtraAD<T>(string tag) where T : class,IAD    
        {
            IAD reseult;
            if(!ExtraAD.TryGetValue(tag, out reseult))
            {
                ConsoleLog($"不存在{tag}对应的广告");
                return null;
            }
            else
            {
                return ExtraAD[tag] as T;
            }
        }


        void InitAD(IADPlayer player, Action OnInited)
        {
            SplashEvent = new EventHolder();
            BannerEvent = new EventHolder();
            InterEvent = new EventHolder();
            RewardEvent = new EventHolder();
            RewardEvent.onFail += (s, e) => IsLoadingRewardAD = false;
            RewardEvent.onShow += (s, e) => IsLoadingRewardAD = false;
            NativeEvent = new EventHolder();
            ADPlayer = player;
            ADPlayer.OnInited += () => ADPlayer.ShowSplash();
            ADPlayer.OnInited += OnInited;
            ADPlayer.Init();
            if (player.IsTest)
            {
                player.Test();
            }
        }
        public EventHolder SplashEvent { get; private set; }
        public EventHolder BannerEvent { get; private set; }
        public EventHolder InterEvent { get; private set; }
        public EventHolder RewardEvent { get; private set; }
        public bool IsLoadingRewardAD { get; private set; }
        public EventHolder NativeEvent { get; private set; }
        public IADPlayer ADPlayer { get; private set; }
        public void ShowBanner(EventHolder eventHolder = null)
        {
            ADPlayer.ShowBanner(eventHolder);
        }
        public void CloseBanner()
        {
            ADPlayer.CloseBanner();
        }
        public void ShowInter(EventHolder eventHolder = null)
        {
            ADPlayer.ShowInter(eventHolder);
        }
        public void ShowReward(Action reward, EventHolder eventHolder = null)
        {
            IsLoadingRewardAD = true;
            ADPlayer.ShowReward(reward, eventHolder);
        }
        public void ShowNative(EventHolder eventHolder = null)
        {
            ADPlayer.ShowNative(eventHolder);
        }
        public void CloseNative()
        {
            ADPlayer.CloseNative();
        }
        #endregion
        public void ConsoleLog(string str)
        {
            GameExtension.Logger.Log($"SDKListener:{str}");
        }
        public void DelayInvoke(float seconds, Action action)
        {
            StartCoroutine(IEDelayInvoke(seconds, action));
        }
        IEnumerator IEDelayInvoke(float time, Action action)
        {
            if (action != null)
            {
                yield return new WaitForSecondsRealtime(time);
                action.Invoke();
            }
            else
            {
                ConsoleLog("Action为空");
            }
        }

        bool isShowedSplash;
        private void OnApplicationFocus(bool focus)
        {
#if USE_SDK
            if (focus)
            {
                if (!isShowedSplash)
                {
                    ADPlayer.ShowSplash();
                    isShowedSplash = true;
                }
                else
                {
                    isShowedSplash = false;
                }
            }
#endif
        }
    }
}