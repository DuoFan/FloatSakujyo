using DG.Tweening;
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
    public class RestorePanel : UIPanel
    {
        [SerializeField]
        SkeletonGraphic failTitle;

        /*[SerializeField]
        Image progressBar;
        [SerializeField]
        TMP_Text progressText;*/

        [SerializeField]
        Button cancleBtn;
        [SerializeField]
        Button adBtn;
        [SerializeField]
        Button shareBtn;

        private void Awake()
        {
            cancleBtn.onClick.AddListener(Cancel);
            adBtn.onClick.AddListener(OpenRestoreCheckPanel);
            shareBtn.onClick.AddListener(OpenRestoreCheckPanel);
        }

        public override void OnShowStart()
        {
            base.OnShowStart();

            var color = failTitle.color;
            color.a = 0;
            failTitle.color = color;
            failTitle.DOFade(1, 0.25f);

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
            shareBtn.gameObject.CheckActiveSelf(false);
            adBtn.gameObject.CheckActiveSelf(true);
#endif
        }

        void Close()
        {
            GameUIManager.Instance.ClosePanel(this, 0);
        }

        void Cancel()
        {
            Close();
            GameUIManager.Instance.OpenFailPanel();
        }
        
        void OpenRestoreCheckPanel()
        {
            Close();
            GameUIManager.Instance.OpenRestoreCheckPanel();
        }

    }
}
