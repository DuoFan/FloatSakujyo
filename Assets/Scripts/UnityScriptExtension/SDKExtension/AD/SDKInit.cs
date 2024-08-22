using GameExtension;
using SDKExtension;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinout
{
    public class SDKInit : MonoBehaviour
    {

        private void Awake()
        {
            SDKListener.Init(new StubAD(), null);
        }
    }
}

