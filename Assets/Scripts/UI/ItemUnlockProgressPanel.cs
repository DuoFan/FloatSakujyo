using FloatSakujyo.Audio;
using FloatSakujyo.Game;
using FloatSakujyo.Level;
using GameExtension;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FloatSakujyo.UI
{
    public class ItemUnlockProgressPanel : CompletePanel
    {
        [SerializeField]
        Image ballImage;
        public Image BallImg => ballImage;
        [SerializeField]
        Image completedItemImg;

        [SerializeField]
        TMP_Text progressText;

        [SerializeField]
        Animator animator;

        [SerializeField]
        SkeletonGraphic fairyBall;

        public void OnLevelStart()
        {
            UpdateProgress();
            GameController.Instance.OnItemTook += UpdateProgress;
        }


        public override void OnShowStart()
        {
            animator.Play("Idle");
        }

        public void UpdateProgress()
        {
            ballImage.fillAmount = GameController.Instance.GetLevelProgress();
            progressText.text = $"进度{(int)GameController.Instance.GetReadablelProgress()}%";
        }

        public void UnlockItem()
        {
            AudioManager.Instance.PlayWin();
            StartCoroutine(IEUnlockItem());
        }

        IEnumerator IEUnlockItem()
        {
            fairyBall.AnimationState.SetAnimation(0, "idle0", false);

            animator.Play("UnlockItem");
            yield return null;
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);

            fairyBall.AnimationState.SetAnimation(0, "idle1", false);
            var time = fairyBall.AnimationState.GetCurrent(0).AnimationEnd;
            yield return new WaitForSeconds(time);
            fairyBall.AnimationState.SetAnimation(0, "idle2", true);
        }
    }
}

