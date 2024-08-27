using FloatSakujyo.Game;
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
        Transform levelSprites;
        List<Image> numImages;

        [SerializeField]
        TMP_Text leftScrewCountText;
        [SerializeField]
        Button pauseBtn;

        [SerializeField]
        HelperItemBtn packageBtn;
        [SerializeField]
        HelperItemBtn rearrangeBtn;
        [SerializeField]
        HelperItemBtn clearNoneSlotGroupBtn;

        [SerializeField]
        GameObject breakPlaneHint;

        [SerializeField]
        Transform topContent;

        public void InitUI()
        {
            numImages = new List<Image>();
           /* pauseBtn.onClick.AddListener(GameUIManager.Instance.OpenSettingPanel);
            GameUIManager.Instance.AdjustPosYBySafeArea(topContent);*/
        }

        public void InitHelperBtns()
        {
            packageBtn.Init(GameController.Instance.CompleteGroup, 2);
            rearrangeBtn.Init(GameController.Instance.Rearrange, 2);
            clearNoneSlotGroupBtn.Init(GameController.Instance.ClearNoneSlotGroupForRestore, 2);
        }

        public void OnLevelStart()
        {

        }

        public void SetLevelID(int levelID)
        {
            var levelIDSprites= CharacterSpriteManager.Instance.GetNumberSprites(levelID);
            for (int i = 0; i < levelIDSprites.Count; i++)
            {
                Image img = null;
                if(i >= numImages.Count)
                {
                    var go = new GameObject();
                    go.transform.SetParent(levelSprites);
                    img = go.AddComponent<Image>();
                    img.rectTransform.sizeDelta = new Vector2(30, 35);
                    img.preserveAspect = true;
                    go.transform.localScale = Vector3.one;
                    numImages.Add(img);
                }
                else
                {
                    img = numImages[i];
                }
                img.sprite = levelIDSprites[i];
            }

            for (int i = numImages.Count - 1; i >= levelIDSprites.Count; i--)
            {
                Destroy(numImages[i].gameObject);
                numImages.RemoveAt(i);
            }
        }

        public void SetLeftScrewCount(int leftScrewCount)
        {
            leftScrewCountText.text = $"剩余螺丝：{leftScrewCount}";
        }

        public void ResetHelperItems()
        {
            packageBtn.ResetCount();
            rearrangeBtn.ResetCount();
            clearNoneSlotGroupBtn.ResetCount();
        }

        public void ShowBreakPlaneHint()
        {
            breakPlaneHint.CheckActiveSelf(true);
        }
        public void HideBreakPlaneHint()
        {
            breakPlaneHint.CheckActiveSelf(false);
        }
    }
}

