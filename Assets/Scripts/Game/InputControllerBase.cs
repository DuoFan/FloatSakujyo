using GameExtension;
using FloatSakujyo.Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FloatSakujyo.Game
{
    public abstract class InputControllerBase
    {
        protected Vector2 lastMousePosition;
        public LevelEntityBase LevelEntity { get; private set; }
        public void SetLevelEntity(LevelEntityBase levelEntity)
        {
            this.LevelEntity = levelEntity;
        }
        public abstract Item TryCatchItem();
    }
}
