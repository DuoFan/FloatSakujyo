using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    [System.Serializable]
    public class LayerCollision 
    {
        public int layer;
        public uint collisionMask;
        public override string ToString()
        {
            return $"Layer:{layer},CollisionMask:{Convert.ToString(collisionMask, 2)}";
        }
    }
}
