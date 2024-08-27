using DG.Tweening;
using FloatSakujyo.Game;
using GameExtension;
using SDKExtension;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WeChatWASM;

namespace FloatSakujyo.UI
{
    public class CompletePanel : UIPanel,IInitUI
    {
        [SerializeField]
        SkeletonGraphic completeTitle;

        [SerializeField]
        Button getBtn;
        [SerializeField]
        Button adGetBtn;

        [SerializeField]
        TMP_Text getCoinText;
        [SerializeField]
        TMP_Text adGetCoinText;

        public void InitUI()
        {
            getBtn.onClick.AddListener(() => GetCoinThenNextLevel(1));
            adGetBtn.onClick.AddListener(ADForGetMultipleCoinThenNextLevel);

            getCoinText.text = GameConfig.Instance.CompletedLevelCoin.ToString();
            adGetCoinText.text = (GameConfig.Instance.CompletedLevelCoin * GameConfig.Instance.CompletedLevelCoinMultiple).ToString();
        }

        private void GetCoinThenNextLevel(int coinMultiple)
        {
            Close();
            GameController.Instance.NextLevel();
        }
        private void ADForGetMultipleCoinThenNextLevel()
        {
            SDKListener.Instance.ShowReward(() =>
            {
                GetCoinThenNextLevel(3);
            });
        }

        public override void OnShowStart()
        {
            base.OnShowStart();

            var color = completeTitle.color;
            color.a = 0;
            completeTitle.color = color;
            completeTitle.DOFade(1, 0.25f);

#if WEI_XIN
            //仅在第一次分享时显示分享按钮
            var isSharedForRestore = GameDataManager.Instance.IsSharedForRestore();
            if (!isSharedForRestore)
            {
                shareBtn.gameObject.CheckActiveSelf(true);
                adBtn.gameObject.CheckActiveSelf(false);
            }
            else
            {
                shareBtn.gameObject.CheckActiveSelf(false);
                adBtn.gameObject.CheckActiveSelf(true);
            }
#else
            adGetBtn.gameObject.CheckActiveSelf(true);
#endif
        }

        void Close()
        {
            GameUIManager.Instance.ClosePanel(this, 0);
        }
      
    }
}
