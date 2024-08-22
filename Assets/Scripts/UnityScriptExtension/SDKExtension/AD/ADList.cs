using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDKExtension
{
    public abstract class ADList<T> where T : IAD
    {
        public List<T> ads;
        protected int index;
        public float Interval { get; protected set; }
        public bool IsWaitForInterval { get; protected set; }
        public void Set(List<T> ads)
        {
            this.ads = ads;
        }
        protected int FindLoadedAD()
        {
            for (int i = index; i < index + ads.Count; i++)
            {
                var j = i % ads.Count;
                if (ads[j].IsLoaded)
                {
                    return j;
                }
                else
                {
                    ads[j].Load(ads[j].LoadTimer.loadTime);
                }
            }
            return -1;
        }
        public IAD GetLastPlayedAD()
        {
            if (index < 0)
            {
                return null;
            }
            else
            {
                return ads[index];
            }
        }
        public void SetInterval(float interval)
        {
            Interval = interval;
        }
        protected void StartInterval()
        {
            if(Interval > 0)
            {
                IsWaitForInterval = true;
                SDKListener.Instance.DelayInvoke(Interval, () =>
                {
                    IsWaitForInterval = false;
                });
            }
        }
    }

    public class BannerADList : ADList<IBannerAD>, IShowAD,ICloseAD
    {
        public void Show(EventHolder eventHolder = null)
        {
            if (IsWaitForInterval)
            {
                SDKListener.Instance.ConsoleLog("处于横幅广告间隔中");
                return;
            }

            var lastAD = GetLastPlayedAD();
            if(lastAD is IBannerAD banner && banner.IsShowing)
            {
                SDKListener.Instance.ConsoleLog("上一个横幅还在显示");
                return;
            }


            var i = FindLoadedAD();

            if (i < 0)
            {
                i = 0;
                SDKListener.Instance.ConsoleLog("没有已加载的横幅广告，开始加载第一个广告");
            }

            index = i;
            ads[index].Show(eventHolder);

            StartInterval();
        }
        public void Close()
        {
            var banner = GetLastPlayedAD();
            (banner as IBannerAD)?.Close();
        }
    }
    public class InterADList : ADList<IInterAD>, IShowAD
    {
        public void Show(EventHolder eventHolder = null)
        {
            if(IsWaitForInterval)
            {
                SDKListener.Instance.ConsoleLog("处于插屏广告间隔中");
                return;
            }

            var i = FindLoadedAD();

            if (i < 0)
            {
                SDKListener.Instance.ConsoleLog("没有已加载的插屏广告");
                return;
            }

            index = i;
            ads[index].Show(eventHolder);

            StartInterval();
        }
    }
}

