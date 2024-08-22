using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace SDKExtension
{
    public class StubAD : ISDK, IADPlayer
    {
        public Action OnInited { get; set; }
        public bool IsTest { get; set; }

        EventHolder bannerEvent;
        EventHolder nativeEvent;

        public void Init()
        {
            SDKListener.Instance.ConsoleLog("广告初始化(Stub)");
            OnInited?.Invoke();
        }

        public void Test()
        {
            SDKListener.Instance.ConsoleLog("广告测试模式(Stub)");
        }

        public void ShowSplash(EventHolder eventHolder = null)
        {
            SDKListener.Instance.ConsoleLog("播放开屏广告(Stub)");
            eventHolder?.onShow?.Invoke(this, null);
        }
        public void ShowBanner(EventHolder eventHolder = null)
        {
            SDKListener.Instance.ConsoleLog("播放横幅广告(Stub)");
            bannerEvent = eventHolder;
            bannerEvent?.onShow?.Invoke(this, null);
        }
        public void CloseBanner()
        {
            SDKListener.Instance.ConsoleLog("关闭横幅广告(Stub)");
            bannerEvent?.onClose?.Invoke(this, null);
        }
        public void ShowInter(EventHolder eventHolder = null)
        {
            SDKListener.Instance.ConsoleLog("播放插屏广告(Stub)");
            eventHolder?.onShow?.Invoke(this, null);
        }
        public void ShowReward(Action reward, EventHolder eventHolder = null)
        {
            SDKListener.Instance.ConsoleLog("播放激励广告(Stub)");
            SDKListener.Instance.RewardEvent?.onShow?.Invoke(this, null);
            eventHolder?.onShow?.Invoke(this, null);
            reward?.Invoke();
            VideoShowEventArgs args = new VideoShowEventArgs(true);
            SDKListener.Instance.RewardEvent?.onClose?.Invoke(this, args);
            eventHolder?.onClose?.Invoke(this, args);
        }
        public void ShowNative(EventHolder eventHolder = null)
        {
            SDKListener.Instance.ConsoleLog("播放原生广告(Stub)");
            nativeEvent = eventHolder;
            nativeEvent?.onShow?.Invoke(this, null);
        }

        public void CloseNative()
        {
            SDKListener.Instance.ConsoleLog("关闭原生广告(Stub)");
            nativeEvent?.onClose?.Invoke(this, null);
        }
    }
}
