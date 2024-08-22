using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class LayerManager : MonoBehaviour
    {
        public static LayerManager Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new LayerManager();
                }
                return instance;
            }
        }
        private static LayerManager instance;
        LayerManager() { }
        public void SetLayerCollision(LayerCollision layerCollision)
        {
            uint mask = 1;
            bool isCollide;
            for (int i = 0; i < 32; i++)
            {
                isCollide = ((layerCollision.collisionMask >> i) & mask) == 1;
                Physics.IgnoreLayerCollision(layerCollision.layer, i, !isCollide);
            }
        }
        public void SetLayerCollisions(LayerCollision[] layerCollisions)
        {
            for (int i = 0; i < layerCollisions.Length; i++)
            {
                SetLayerCollision(layerCollisions[i]);
            }
        }
        public void SetLayerCollisions(uint[] collisionMasks)
        {
            LayerCollision layerCollision = new LayerCollision();
            for (int i = 0; i < collisionMasks.Length; i++)
            {
                layerCollision.layer = i;
                layerCollision.collisionMask = collisionMasks[i];
                SetLayerCollision(layerCollision);
            }
        }
    }
}
