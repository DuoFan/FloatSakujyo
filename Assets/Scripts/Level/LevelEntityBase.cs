using GameExtension;
using FloatSakujyo.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace FloatSakujyo.Level
{
    public abstract class LevelEntityBase : MonoBehaviour, IDisposable
    {
        public LevelData LevelData { get; protected set; }

        public abstract IEnumerator Init(LevelData _levelData);

        public virtual void Dispose()
        {
            Destroy(gameObject);
        }
    }
}
