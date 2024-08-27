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
        int totalCount;
        [SerializeField]
        Image usedCountImg;
        [SerializeField]
        Image totalCountImg;
        [SerializeField]
        GameObject useableHint;

        [SerializeField]
        Sprite textSprite;
        public Sprite TextSprite => textSprite;

        [SerializeField]
        Sprite itemIconSprite;
        public Sprite ItemIconSprite => itemIconSprite;

        Button button;

        Action help;
        int usedCount;
        public void Init(Action _help, int _totalCount)
        {
            help = _help;
            totalCount = _totalCount;
            button = GetComponent<Button>();
            button.onClick.AddListener(TryOpenPanelForSelf);
            //默认totalCount < 10
            totalCountImg.sprite = CharacterSpriteManager.Instance.GetNumberSprites(totalCount)[0];
            ResetCount();
        }

        public void ResetCount()
        {
            usedCount = 0;
            usedCountImg.sprite = CharacterSpriteManager.Instance.GetNumberSprites(usedCount)[0];
            useableHint.CheckActiveSelf(usedCount < totalCount);
        }

        void TryOpenPanelForSelf()
        {
            if (usedCount < totalCount)
            {
                //GameUIManager.Instance.OpenHelperItemPanel(this);
            }
        }

        public void UseHelp()
        {
            help?.Invoke();
            usedCount++;
            usedCountImg.sprite = CharacterSpriteManager.Instance.GetNumberSprites(usedCount)[0];
            useableHint.CheckActiveSelf(usedCount < totalCount);
        }
    }
}

