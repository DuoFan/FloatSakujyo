using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDKExtension
{
    public interface IShowing
    {
        bool IsShowing { get; set; }
    }
    public interface IShowAD
    {
        void Show(EventHolder eventHolder = null);
    }
    public interface ICloseAD
    {
        void Close();
    }
    public interface IAD
    {
        LoadTimer LoadTimer { get; set; }
        EventHolder EventHolder { get; set; }
        bool IsLoaded { get; set; }
        void Load(float delay);
    }
    public interface ISplashAD:IAD, IShowAD
    {
    }
    public interface IBannerAD : IAD, IShowing, ICloseAD, IShowAD
    {

    }
    public interface IInterAD : IAD, IShowAD
    {
    }
    public interface IRewardAD : IAD
    {
        Action Reward { get; set; }
        void Show(Action reward,EventHolder eventHolder = null);
    }
    public interface INativeAD : IAD, IShowing, ICloseAD, IShowAD
    {

    }
    public interface IADPlayer : ISDK
    {
        void ShowSplash(EventHolder eventHolder = null);
        void ShowBanner(EventHolder eventHolder = null);
        void CloseBanner();
        void ShowInter(EventHolder eventHolder = null);
        void ShowReward(Action reward, EventHolder eventHolder = null);
        void ShowNative(EventHolder eventHolder = null);
        void CloseNative();
    }
}
