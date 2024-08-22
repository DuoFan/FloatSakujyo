using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameExtension
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class FlySprite : FlyObject
    {
        [SerializeField] SpriteRenderer sprite;

        public override void OnStartFly()
        {
            if(sprite != null && !sprite.enabled)
            {
                sprite.enabled = true;
            }
        }


        public override void OnEndFly()
        {
            if (sprite != null && sprite.enabled)
            {
                sprite.enabled = false;
            }
        }
    }
}
