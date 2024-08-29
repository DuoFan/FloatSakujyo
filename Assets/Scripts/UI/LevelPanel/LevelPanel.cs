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
        Image[] levelSteps;

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

        public void OnLevelStart()
        {
            SetLevelID(GameController.Instance.LevelData.ID);

            InitHelperBtns();

            InitLevelSteps();

            UpdateLevelProgress();

            GameController.Instance.OnItemTook += UpdateLevelProgress;
        }
        void InitHelperBtns()
        {
            packageBtn.Init(GameController.Instance.CompleteGroup);
            rearrangeBtn.Init(GameController.Instance.Rearrange);
            clearNoneSlotGroupBtn.Init(GameController.Instance.ClearNoneSlotGroup);
        }

        void InitLevelSteps()
        {
            var progressWidth = levelProgress.rectTransform.rect.width;
            var levelData = GameController.Instance.LevelData;
            var totalItemCount = GameController.Instance.TotalItemCount;
            int itemCount = 0;
            for (int i = 0; i < levelSteps.Length; i++)
            {
                itemCount += levelData.SubLevelDatas[i].TotalItemCount;
                levelSteps[i].rectTransform.anchoredPosition = Vector3.right * (progressWidth * itemCount / totalItemCount - levelSteps[i].rectTransform.rect.width * 0.5f 
                    * levelSteps[i].rectTransform.localScale.x);
            }
        }

        void SetLevelID(int levelID)
        {
            levelText.text = levelID.ToString();
        }

        void UpdateLevelProgress()
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

