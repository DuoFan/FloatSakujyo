using DG.Tweening;
using FloatSakujyo.Game;
using FloatSakujyo.Level;
using GameExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FloatSakujyo.UI
{
    public class LevelPanel : UIPanel, IInitUI
    {
        [SerializeField]
        TextMeshProUGUI levelText;
        [SerializeField]
        Image levelProgress;
        Tween levelProgressTween;

        [SerializeField]
        Button pauseBtn;

        [SerializeField]
        HelperItemBtn packageBtn;
        [SerializeField]
        HelperItemBtn rearrangeBtn;
        [SerializeField]
        HelperItemBtn clearNoneSlotGroupBtn;

        [SerializeField]
        Transform topContent;

        public void InitUI()
        {
             pauseBtn.onClick.AddListener(GameUIManager.Instance.OpenSettingPanel);
             /*GameUIManager.Instance.AdjustPosYBySafeArea(topContent);*/
        }

        public void InitHelperBtns()
        {
            packageBtn.Init(GameController.Instance.CompleteGroup);
            rearrangeBtn.Init(GameController.Instance.Rearrange);
            clearNoneSlotGroupBtn.Init(GameController.Instance.ClearNoneSlotGroup);
        }

        public void OnLevelStart()
        {
            SetLevelID(GameController.Instance.LevelEntity.LevelData.ID);

            InitHelperBtns();

            UpdateLevelProgress();
            GameController.Instance.OnItemTook += UpdateLevelProgress;
        }

        public void SetLevelID(int levelID)
        {
            levelText.text = levelID.ToString();
        }

        public void UpdateLevelProgress()
        {
            if(levelProgressTween != null)
            {
                levelProgressTween.Pause();
                levelProgressTween.Kill();
            }

            levelProgressTween = levelProgress.DOFillAmount(GameController.Instance.GetLevelProgress(), 0.5f);
        }
    }
}

