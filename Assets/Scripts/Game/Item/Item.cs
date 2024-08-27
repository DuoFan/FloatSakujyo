using GameExtension;
using System;
using System.Collections;
using UnityEngine;

namespace FloatSakujyo.Game
{
    public class Item : MonoBehaviour
    {
        [SerializeField]
        TrailRenderer trail;

        [SerializeField]
        protected ItemColor itemColor;
        public ItemColor ItemColor => itemColor;
        public ItemGeneration ItemGeneration { get; private set; }

        public Rigidbody Rigidbody
        {
            get
            {
                if(rigidbody == null)
                {
                    rigidbody = GetComponent<Rigidbody>();
                }
                return rigidbody;
            }
        }

        new Rigidbody rigidbody;

        public void SetGeneration(ItemGeneration itemGeneration)
        {
            ItemGeneration = itemGeneration;
        }

        public void DisablePhysics()
        {
            Rigidbody.isKinematic = true;
        }

        public void EnablePhysics()
        {
            Rigidbody.isKinematic = false;
        }

        public void ShowTrail()
        {
            trail.gameObject.CheckActiveSelf(true);
        }

        public void HideTrail()
        {
            trail.gameObject.CheckActiveSelf(false);
        }
    }
}

