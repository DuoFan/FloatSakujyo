using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GameExtension;
using System.Text;
using UnityEngine.UIElements;
using System;

namespace EditorExtension
{
    public static class LayerStatistics
    {
        [MenuItem("GameExtension/Tools/LayerCollisionStatistics")]
        public static void LayerCollisionStatistics()
        {
            LayerCollision layerCollision = new LayerCollision();
            uint[] masks = new uint[32];
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < 32; ++i)
            {
                layerCollision.layer = i;
                layerCollision.collisionMask = 0;
                for (int j = 0; j < 32; ++j)
                {
                    if (j > 0)
                    {
                        layerCollision.collisionMask >>= 1;
                    }
                    if (!Physics.GetIgnoreLayerCollision(i, j))
                    {
                        layerCollision.collisionMask += 0x80000000;
                    }
                }
                stringBuilder.Append(layerCollision.ToString());
                stringBuilder.Append('\n');
                masks[i] = layerCollision.collisionMask;
            }


            stringBuilder.Append('{');
            for (int i = 0; i < masks.Length; i++)
            {
                stringBuilder.Append($"0x{Convert.ToString(masks[i], 16)}");
                if(i  < masks.Length - 1)
                {
                    stringBuilder.Append(", ");
                }
            }
            stringBuilder.Append('}');

            Debug.Log(stringBuilder.ToString());    
        }
    }
}
