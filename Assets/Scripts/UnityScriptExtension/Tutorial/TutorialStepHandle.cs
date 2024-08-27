using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class TutorialStepHandle : DynamicDataProxy<int>
    {
        public const int NEVER_START = -1;
        public bool IsNeverStart => Get() == NEVER_START;
        public TutorialStepHandle(Action<int> _setter, Func<int> _getter) : base(_setter, _getter)
        {

        }
    }
}

