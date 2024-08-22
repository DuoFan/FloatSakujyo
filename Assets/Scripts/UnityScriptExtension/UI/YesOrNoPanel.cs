using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using GameExtension;
using UnityEngine.UI;

namespace GameExtension
{
    public class YesOrNoPanel : UIPanel
    {
        public static YesOrNoPanel instance;
        [SerializeField] Text title;
        [SerializeField] Button yesBtn;
        [SerializeField] Button noBtn;
        Action onYes;
        Action onNo;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            yesBtn.onClick.AddListener(() =>
            {
                onYes?.Invoke();
                onYes = null;
                UIManager.Instance.ClosePanel(this);
            });

            noBtn.onClick.AddListener(() =>
            {
                onNo?.Invoke();
                onNo = null;
                UIManager.Instance.ClosePanel(this);
            });
        }

        public override void OnCloseStart()
        {
            onYes = onNo = null;
            base.OnCloseStart();
        }


        public void Show(string _title, Action _onYes, Action _onNo)
        {
            title.text = _title;
            onYes = _onYes;
            onNo = _onNo;
            UIManager.Instance.OpenPanel(this);
        }

    }
}
