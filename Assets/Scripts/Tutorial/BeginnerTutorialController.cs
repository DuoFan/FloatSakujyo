using GameExtension;
using FloatSakujyo.Game;
using FloatSakujyo.Level;
using FloatSakujyo.SaveData;
using FloatSakujyo.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Device;

namespace FloatSakujyo.Tutorial
{
    public class BeginnerTutorialController : TutorialControllerBase
    {
        /*public override string TutorialKey => "BeginnerTutorial";

         BeginnerTutorialPanel tutorialPanel;
         Coroutine tutorialCo;

         public void StartTutorialCoroutineWhenNotCompleted()
         {
             tutorialCo = GameController.Instance.StartCoroutine(StartTutorialWhenNotCompleted());
         }

         public override IEnumerator StartTutorial()
         {
             GameController.Instance.FreezeGameplay();
             var inputController = GameController.Instance.InputController;
             var levelEntity = GameController.Instance.LevelEntity;
             tutorialPanel = GameUIManager.Instance.OpenBeginnerTutorialPanel();

             tutorialPanel.ShowTakeScrewContent();

             var takeScrewTipImg = tutorialPanel.takeScrewTipImg;
             takeScrewTipImg.rectTransform.anchoredPosition = Vector3.up * (takeScrewTipImg.rectTransform.parent as RectTransform).rect.height * 0.62f;

             int count = 9;
             while (count > 0)
             {
                 yield return null;

                 var screw = inputController.TryCatchItem<FloatSakujyo.Game.Screw>();

                 if (screw != null)
                 {
                     levelEntity.TryTakeScrew(screw);
                     count--;
                     //拔掉第一颗螺丝后隐藏手指
                     tutorialPanel.fingerImg.gameObject.CheckActiveSelf(false);
                 }

                 inputController.HandleRotate();
             }

             tutorialPanel.HideTakeScrewContent();

             yield return new WaitForSeconds(0.5f);

             tutorialPanel.ShowRotationContent();

             var rotationImg = tutorialPanel.rotationImg;
             rotationImg.rectTransform.anchoredPosition = Vector3.up * (rotationImg.rectTransform.parent as RectTransform).rect.height * 0.18f;

             var rotationTipImg = tutorialPanel.rotationTipImg;
             rotationTipImg.rectTransform.anchoredPosition = Vector3.up * (rotationTipImg.rectTransform.parent as RectTransform).rect.height * 0.62f;

             while (!inputController.HandleRotate())
             {
                 yield return null;
             }

             tutorialPanel.HideRotationContent();

             GameUIManager.Instance.CloseBeginnerTutorialPanel();

             GameController.Instance.TryResumeGameplay();

             GameDataManager.Instance.GetTutorialStepHandle(TutorialKey).Set(ENDING);
         }

         public void Dispose()
         {
             if(tutorialPanel != null)
             {
                 tutorialPanel.HideTakeScrewContent();
                 tutorialPanel.HideRotationContent();
                 GameUIManager.Instance.CloseBeginnerTutorialPanel();
             }

             if(tutorialCo != null)
             {
                 GameController.Instance.StopCoroutine(tutorialCo);
             }
         }*/
        public override string TutorialKey => throw new System.NotImplementedException();

        public override IEnumerator StartTutorial()
        {
            throw new System.NotImplementedException();
        }
    }
}
