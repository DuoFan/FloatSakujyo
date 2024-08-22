using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class PanelEventHandler
    {
        public PanelEventStage onShow;
        public Action onOverlay;
        public Action onResume;
        public PanelEventStage onClose;
    }

    public class PanelEventStage
    {
        public Action onStart;
        public Action onEnd;
    }
}
