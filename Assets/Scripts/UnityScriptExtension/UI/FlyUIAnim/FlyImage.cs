using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameExtension
{
    [RequireComponent(typeof(Image))]
    public class FlyImage : FlyObject
    {
        [SerializeField] Image image;

        public override void OnStartFly()
        {
            if(image != null && !image.enabled)
            {
                image.enabled = true;
            }
        }


        public override void OnEndFly()
        {
            if (image != null && image.enabled)
            {
                image.enabled = false;
            }
        }
    }
}
