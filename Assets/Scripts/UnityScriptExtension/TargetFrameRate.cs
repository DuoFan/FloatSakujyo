using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class TargetFrameRate : MonoBehaviour
    {
        [SerializeField]
        int targetFrameRate;
        private void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
        }
    }
}

