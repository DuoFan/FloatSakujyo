using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDKExtension
{
    public class EventHolder
    {
        public EventHandler onShow;
        public EventHandler onClose;
        public EventHandler onClick;
        public EventHandler onPaid;
        public EventHandler onFail;
    }

    public class VideoShowEventArgs : EventArgs
    {
        public bool IsFinish { get; private set; }
        public VideoShowEventArgs(bool isFinish)
        {
            IsFinish = isFinish;
        }
    }
}
