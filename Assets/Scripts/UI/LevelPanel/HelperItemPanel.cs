using GameExtension;
using SDKExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FloatSakujyo.UI
{
    public class HelperItemPanel : UIPanel,IInitUI
    {
        [SerializeField]
        Button helpBtn;
        [SerializeField]
        Image textImg;
        [SerializeField]
        Image itemIconImg;

        HelperItemBtn helperItemBtn;

        public void InitUI()
        {
            closeBtn.onClick.AddListener(Close);
            helpBtn.onClick.AddListener(TryHelp);
        }

        public void Open(HelperItemBtn _helperItemBtn)
        {
            helperItemBtn = _helperItemBtn;
            //textImg.sprite = helperItemBtn.TextSprite;
            itemIconImg.sprite = helperItemBtn.ItemIconSprite;
            GameUIManager.Instance.OpenPanel(this);
        }

        void Close()
        {
            GameUIManager.Instance.ClosePanel(this);
        }

        void TryHelp()
        {
            SDKListener.Instance.ShowReward(() =>
            {
                helperItemBtn.UseHelp();
                Close();
            });
        }
    }
}

