using DG.Tweening;
using FloatSakujyo.Audio;
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
    public class FailPanel : UIPanel
    {
        [SerializeField]
        SkeletonGraphic failTitle;

        /*[SerializeField]
        Image progressBar;
        [SerializeField]
        TMP_Text progressText;*/

        [SerializeField]
        Button backBtn;
        [SerializeField]
        Button restartBtn;

        private void Awake()
        {
            backBtn.onClick.AddListener(Back);
            restartBtn.onClick.AddListener(OpenRestartCheckPanel);
        }

        public override void OnShowStart()
        {
            base.OnShowStart();

            var color = failTitle.color;
            color.a = 0;
            failTitle.color = color;
            failTitle.DOFade(1, 0.25f);

            AudioManager.Instance.PlayFail();
        }

        void Close()
        {
            GameUIManager.Instance.ClosePanel(this, 0);
        }

        void Back()
        {
            Close();
            GameController.Instance.BackToMainPage();
        }
        
        void OpenRestartCheckPanel()
        {
            Close();
            GameUIManager.Instance.OpenRestartCheckPanel();
        }

    }
}
