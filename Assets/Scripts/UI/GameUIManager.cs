using GameExtension;
using FloatSakujyo.Game;
using System;
using System.Collections;
using System.Numerics;
using System.Text;
using TMPro;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace FloatSakujyo.UI
{
    public class GameUIManager : UIManager, IGameInitializer
    {
        public static new GameUIManager Instance => instance;

        static GameUIManager instance;

        [SerializeField]
        Canvas canvas;
        /*
        [SerializeField]
        LevelPanel levelPanel;
        [SerializeField]
        BuildProgressPanel buildProgressPanel;
        public BuildProgressPanel BuildProgressPanel => buildProgressPanel;*/

        [SerializeField]
        RestorePanel restorePanel;
        [SerializeField]
        RestoreCheckPanel restoreCheckPanel;
        [SerializeField]
        FailPanel failPanel;
        [SerializeField]
        CompletePanel completePanel;
        /*[SerializeField]
        SettingPanel settingPanel;
        [SerializeField]
        HelperItemPanel helperItemPanel;
        [SerializeField]
        ToastPanel toastPanel;

        [SerializeField]
        FlyAnimPlayer flyAnimPlayer;*/

        protected override void Awake()
        {
            if (Singleton.KeepWhenSingletonNull(ref instance, this))
            {
                base.Awake();
                //UIUtils.InitSafeArea();
            }
        }

        public IEnumerator InitializeGame()
        {
            canvas.worldCamera = CameraController.Instance.Camera;
           /* levelPanel?.InitUI();
            helperItemPanel?.InitUI();*/
            failPanel?.InitUI();
            /*beginnerTutorialPanel?.InitUI();
            settingPanel?.InitUI();
            buildProgressPanel?.InitUI();*/
            yield break;
        }

        /*public void AdjustPosYBySafeArea(Transform transform)
        {
            var rectTransform = transform as RectTransform;
            var posY = rectTransform.anchoredPosition.y - UIUtils.SafeAreaTop;
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, posY);
        }

        public LevelPanel GetLevelPanel()
        {
            GetUI(ref levelPanel);
            return levelPanel;
        }

        public LevelPanel OpenLevelPanel()
        {
            OpenPanel(ref levelPanel, 0);
            return levelPanel;
        }
        public void CloseLevelPanel()
        {
            ClosePanel(ref levelPanel, 0);
        }
        public BuildProgressPanel OpenBuildProgressPanel(City.CityConfigData cityConfigData, int buildingIndex)
        {
            GetUI(ref buildProgressPanel).Open(cityConfigData, buildingIndex);
            return buildProgressPanel;
        }
        public void CloseBuildProgressPanel()
        {
            ClosePanel(ref buildProgressPanel, 0);
        }*/

        public void OpenRestorePanel()
        {
            OpenPanel(ref restorePanel);
        }
        public void OpenRestoreCheckPanel()
        {
            OpenPanel(ref restoreCheckPanel);
        }

        public void OpenFailPanel()
        {
            OpenPanel(ref failPanel, 0);
        }

        public void OpenCompletePanel()
        {
            OpenPanel(ref completePanel);
        }

        /*public void OpenSettingPanel()
        {
            OpenPanel(ref settingPanel);
        }

        public void OpenHelperItemPanel(HelperItemBtn helperItemBtn)
        {
            GetUI(ref helperItemPanel).Open(helperItemBtn);
        }

        public void ShowToast(string text)
        {
            GetUI(ref toastPanel).ShowToast(text);
        }

        #region 

        public void PlayFlyGold(Transform start)
        {
            var target = GetUI(ref buildProgressPanel).BuildingImg.transform;
            flyAnimPlayer.PlayFlyGold(start, target);
        }

        #endregion

        #region Tutorial

        [SerializeField]
        BeginnerTutorialPanel beginnerTutorialPanel;

        public BeginnerTutorialPanel OpenBeginnerTutorialPanel()
        {
            OpenPanel(ref beginnerTutorialPanel, 0);
            return beginnerTutorialPanel;
        }

        public void CloseBeginnerTutorialPanel()
        {
            ClosePanel(ref beginnerTutorialPanel, 0);
        }

        #endregion*/
    }
}

