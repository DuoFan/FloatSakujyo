using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameExtension
{
    public class LoadingPanel : SingletonMonoBase<LoadingPanel>
    {
        [SerializeField]
        RectTransform background;

        [SerializeField]
        Image loadingBar;

        [SerializeField]
        Image autoLoadingBar;

        [SerializeField]
        TMP_Text loadingText;

        public float Progress
        {
            get => loadingBar.fillAmount;
            set => loadingBar.fillAmount = value;
        }

        //之前使用动画需要手动缩放，现在使用图片自适应
        //Android版本改成手动缩放
        protected virtual void Start()
        {
            ScaleBackground();
        }

        void ScaleBackground()
        {
            if (background != null)
            {
                var width = background.rect.width;
                var height = background.rect.height;
                var screenWidth = Screen.width;
                var screenHeight = Screen.height;
                var scale = Mathf.Max(screenWidth / width, screenHeight / height);
                if (scale < 1)
                {
                    scale = Mathf.Max(width / screenWidth, height / screenHeight);
                }
                background.localScale = new Vector3(scale, scale, 1);
                GameExtension.Logger.Log($"LoadingBar scale: {scale}:{background.rect.width * scale}:{background.rect.height * scale}");
            }
        }

        public void Show()
        {
            gameObject.CheckActiveSelf(true);
        }
        public void Hide()
        {
            gameObject.CheckActiveSelf(false);
        }

        public void SetLoadingText(string text)
        {
            if (loadingText != null)
            {
                loadingText.text = text;
            }
        }

        void FixedUpdate()
        {
            if(autoLoadingBar != null)
            {
                autoLoadingBar.fillAmount = (autoLoadingBar.fillAmount + 0.03f) % 1;
            }
        }
    }
}
