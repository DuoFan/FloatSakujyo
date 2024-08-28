using FloatSakujyo.SaveData;
using GameExtension;
using SDKExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace FloatSakujyo.UI
{
    [RequireComponent(typeof(Button))]
    public class HelperItemBtn : MonoBehaviour
    {
        [SerializeField]
        Image useableCount;
        [SerializeField]
        Image adIcon;

        [SerializeField]
        Sprite itemIconSprite;
        public Sprite ItemIconSprite => itemIconSprite;

        Button button;

        Action help;

        [SerializeField]
        HelperType helperType;

        public void Init(Action _help)
        {
            help = _help;
            button = GetComponent<Button>();
            button.onClick.RemoveAllListeners();    
            button.onClick.AddListener(TryUseHelp);
            CheckUseableCount();
        }

        public void CheckUseableCount()
        {
            var helperCount = GameDataManager.Instance.GetHelperCount(helperType);
            useableCount.sprite = CharacterSpriteManager.Instance.GetNumberSprites(helperCount)[0];
            useableCount.gameObject.CheckActiveSelf(helperCount > 0);
            adIcon.gameObject.CheckActiveSelf(helperCount <= 0);
        }

        public void TryUseHelp()
        {
            var helperCount = GameDataManager.Instance.GetHelperCount(helperType);
            if (helperCount > 0)
            {
                UseHelp();
                GameDataManager.Instance.SubHelperCount(helperType);
                CheckUseableCount();
            }
            else
            {
                SDKListener.Instance.ShowReward(() =>
                {
                    UseHelp();
                });
            }
        }

        public void UseHelp()
        {
            help?.Invoke();
        }
    }
}

