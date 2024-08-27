using GameExtension;
using FloatSakujyo.Game;
using SDKExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WeChatWASM;
using Spine.Unity;
using DG.Tweening;

namespace FloatSakujyo.UI
{
    public class FailPanel : UIPanel, IInitUI
    {
        [SerializeField]
        SkeletonGraphic failTitle;
        [SerializeField]
        Button restartBtn;
        [SerializeField]
        Button returnBtn;

        public void InitUI()
        {
            restartBtn.onClick.AddListener(Restart);
            returnBtn.onClick.AddListener(ReturnToRestorePanel);
        }

        public override void OnShowStart()
        {
            base.OnShowStart();
            var color = failTitle.color;
            color.a = 0;
            failTitle.color = color;
            failTitle.DOFade(1, 0.25f);
        }

        private void Restart()
        {
            GameUIManager.Instance.ClosePanel(this, 0);
            GameController.Instance.Restart();
        }

        private void ReturnToRestorePanel()
        {
            GameUIManager.Instance.ClosePanel(this, 0);
            GameUIManager.Instance.OpenRestorePanel();
        }
    }
}

