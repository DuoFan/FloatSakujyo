using FloatSakujyo.Audio;
using FloatSakujyo.SaveData;
using GameExtension;
using System;
using System.Collections;
using UnityEngine;
using WeChatWASM;

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
            GetComponent<Collider>().enabled = false;
        }

        public void EnablePhysics()
        {
            Rigidbody.isKinematic = false;
            GetComponent<Collider>().enabled = true;
        }

        public void ShowTrail()
        {
            trail.gameObject.CheckActiveSelf(true);
        }

        public void HideTrail()
        {
            trail.gameObject.CheckActiveSelf(false);
        }

        public void Take()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer && GameDataManager.Instance.GetPlayerPreference().HasVibration)
            {
                WX.VibrateShort(new VibrateShortOption()
                {
                    type = "medium"
                });
            }

            AudioManager.Instance.PlayPoP();
        }
    }
}

