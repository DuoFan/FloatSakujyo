using DG.Tweening;
using FloatSakujyo.Audio;
using FloatSakujyo.Game;
using FloatSakujyo.SaveData;
using GameExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FloatSakujyo.UI
{
    public class SettingPanel : UIPanel, IInitUI
    {
        [SerializeField]
        Button continueBtn;
        [SerializeField]
        Button restartBtn;
        [SerializeField]
        Button backBtn;

        [SerializeField]
        Toggle audioToggle;
        [SerializeField]
        Toggle vibrationToggle;

        public void InitUI()
        {
            continueBtn.onClick.AddListener(() => Close(0.25f));
            restartBtn.onClick.AddListener(Restart);
            backBtn.onClick.AddListener(Back);

            audioToggle.isOn = !GameDataManager.Instance.GetPlayerPreference().IsMute;
            audioToggle.onValueChanged.AddListener(SetAudio);

            vibrationToggle.isOn = GameDataManager.Instance.GetPlayerPreference().HasVibration;
            vibrationToggle.onValueChanged.AddListener(SetVibration);
        }

        void Close(float time)
        {
            GameUIManager.Instance.ClosePanel(this, time);
            GameController.Instance.TryResumeGameplay();
        }

        void Restart()
        {
            GameController.Instance.Restart();
            Close(0.25f);
        }

        private void Back()
        {
            Close(0);
            GameController.Instance.BackToMainPage();
            SceneManager.LoadScene("MainPage");
        }

        void SetAudio(bool isOn)
        {
            AudioManager.Instance.ThemeMute = !isOn;
            AudioManager.Instance.SoundMute = !isOn;
            GameDataManager.Instance.GetPlayerPreference().IsMute = !isOn;
            GameDataManager.Instance.AddBuffer();
        }

        private void SetVibration(bool isOn)
        {
            GameDataManager.Instance.GetPlayerPreference().HasVibration = isOn;
            GameDataManager.Instance.AddBuffer();
        }

        public override void OnShowStart()
        {
            base.OnShowStart();
            GameController.Instance.FreezeGameplay();
        }
    }
}

