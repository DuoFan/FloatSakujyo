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
    public class RestorePanel : UIPanel
    {
        [SerializeField]
        Button adBtn;
        [SerializeField]
        Button shareBtn;

        [SerializeField]
        TMP_Text levelProgressText;

        private void Awake()
        {
            closeBtn.onClick.AddListener(Cancel);
            adBtn.onClick.AddListener(WatchADForRestore);
            shareBtn.onClick.AddListener(ShareForRestore);
        }

        public override void OnShowStart()
        {
            base.OnShowStart();

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

            levelProgressText.text = $"{GameController.Instance.GetReadablelProgress():f0}%"; 
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

        void WatchADForRestore()
        {
            SDKListener.Instance.ShowReward(Restore);
        }

        private void ShareForRestore()
        {
            /*WeiXinShare.FakeShare(() =>
            {
                Restore();
                GameDataManager.Instance.FlagSharedForRestore();
                GameUIManager.Instance.ShowToast("分享成功");
                GameExtension.Logger.Log($"分享成功(界面停留)");
            }, () =>
            {
                string content = UnityEngine.Random.Range(0, 2) == 0 ? "请分享到20人以上的群!" : "分享失败，请不要分享到同一个群";
                WX.ShowModal(new ShowModalOption()
                {
                    cancelText = "取消",
                    confirmText = "去分享",
                    title = "提示",
                    content = content,
                    showCancel = true,
                    success = (res) =>
                    {
                        if (res.confirm)
                        {
                            ShareForRestore();
                        }
                    },
                });
                GameExtension.Logger.Log($"分享失败(三秒内关闭界面)");
            }, 3, new ShareAppMessageOption()
            {
                imageUrlId = "DybIrvWGRMmmtWKqb/n5vg==",
                imageUrl = "https://mmocgame.qpic.cn/wechatgame/1z8oRXvm8alhoibbxia7XBME4zTGfOiblzoicg2G6MbEh19fgcCCZXQoFhf3JWmuiczLa/0",
            });*/
        }

        void Restore()
        {
            GameController.Instance.ClearNoneSlotGroupForRestore();
            Close();
        }

    }
}
