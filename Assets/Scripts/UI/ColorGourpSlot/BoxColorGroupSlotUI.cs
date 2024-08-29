using DG.Tweening;
using GameExtension;
using FloatSakujyo.Game;
using FloatSakujyo.Level;
using SDKExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using FloatSakujyo.Audio;

namespace FloatSakujyo.UI
{
    public class BoxColorGroupSlotUI : ColorGroupSlotUIBase
    {
        [SerializeField]
        MeshRenderer boxRenderer;
        [SerializeField]
        Button btn;

        [SerializeField]
        TMP_Text filledText;

        [SerializeField]
        Animator animator;

        [SerializeField]
        Animator seal;

        public ItemColor ItemColor => Slot.ItemColor;

        public bool IsEnable => Slot != null;

        [SerializeField]
        ItemNeedView itemNeedView;
        public ItemNeedView ItemNeedView => itemNeedView;

        public void Awake()
        {
            btn.onClick.AddListener(TryEnable);
        }

        public override void Init(ColorGroupSlot colorGourpSlot)
        {
            base.Init(colorGourpSlot);

            btn.gameObject.CheckActiveSelf(false);

            DeactivateAllShadow();

            for (int i = 0; i < Slot.Items.Length; i++)
            {
                var item = Slot.Items[i];
                if (item != null)
                {
                    var point = slotPoints[i];
                    item.transform.SetParent(point);

                    item.transform.localPosition = fillLocalPosition;
                    item.transform.localScale = Vector3.one;
                    item.transform.localEulerAngles = fillLocalEulerAngle;
                }
            }

            filledText.text = $"{Slot.TotalSlotCount - Slot.EmptySlotCount}/{Slot.TotalSlotCount}";

            StartCoroutine(colorGroupSlotView.PlayItemNeedViewEnter(this));
        }


        protected override IEnumerator FillItem(Item item, int index)
        {
            ActivateShadow(index);

            filledText.text = $"{Slot.TotalSlotCount - Slot.EmptySlotCount}/{Slot.TotalSlotCount}";

            yield return null;

            var time = 0.3f;

            var point = slotPoints[index];

            item.transform.SetParent(point);

            item.ShowTrail();

            var sequence = DOTween.Sequence();

            sequence.Join(item.transform.DOLocalRotate(Quaternion.Euler(0, 10, 0) * fillLocalEulerAngle, time));
            sequence.Join(item.transform.DOLocalMove(Vector3.up - Vector3.forward * 0.1f, time));
            sequence.Join(item.transform.DOScale(Vector3.one, time));

            yield return sequence.WaitForCompletion();

            time = 0.2f;
            for (int i = index - 1; i >= 0; i--)
            {
                var prevItem = Slot.Items[i];
                if (prevItem == null)
                {
                    continue;
                }
                sequence = DOTween.Sequence();
                sequence.Join(prevItem.transform.DOLocalMoveZ(-0.2f, time));
                sequence.Join(prevItem.transform.DOScale(0.8f * Vector3.one, time));
                sequence.Append(prevItem.transform.DOLocalMove(fillLocalPosition, time));
                sequence.Join(prevItem.transform.DOScale(Vector3.one, time));
                sequence.SetDelay(0.1f * (index - 1 - i));
            }

            sequence = DOTween.Sequence();

            sequence.Join(item.transform.DOLocalMove(fillLocalPosition, time));

            time = 0.2f;
            sequence.Join(item.transform.DOLocalRotate(fillLocalEulerAngle, time));

            yield return new WaitForSecondsRealtime(time / 2f);

            AudioManager.Instance.PlayPut();

            yield return sequence.WaitForCompletion();

            item.HideTrail();
        }

        public override IEnumerator OnCompleted()
        {
            //提前获取自身索引，因为在yield return base.OnCompleted()之后，自身会从slotUIs中移除
            var selfIndex = Array.IndexOf(colorGroupSlotView.SlotUIs, this);

            yield return base.OnCompleted();

            Tween t = null;

            for (int i = 0; i < Slot.Items.Length; i++)
            {
                var item = Slot.Items[i];
                if (item != null)
                {
                    t = item.transform.DOLocalJump(fillLocalPosition, 1, 1, 0.2f).SetDelay(0.1f * i);
                }
            }

            yield return t.WaitForCompletion();

            animator.Play("close");

            AudioManager.Instance.PlayCloseBox();

            //盖子动画时间
            yield return new WaitForSecondsRealtime(0.25f);

            Vector3[] coinPoses = new Vector3[slotPoints.Length];

            for (int i = 0; i < slotPoints.Length; i++)
            {
                coinPoses[i] = slotPoints[i].position;
            }

            Slot.Dispose();

            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);

            filledText.transform.parent.gameObject.CheckActiveSelf(false);

            seal.gameObject.CheckActiveSelf(true);

            yield return new WaitUntil(() => seal.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);

            boxRenderer.gameObject.CheckActiveSelf(false);

            StartCoroutine(colorGroupSlotView.PlayItemNeedViewExit(this, selfIndex));

            yield return colorGroupSlotView.FlyCoins(coinPoses);

            GameObject.Destroy(itemNeedView.gameObject);

            Destroy(gameObject);
        }

        public override void Disable()
        {
            base.Disable();
            boxRenderer.gameObject.CheckActiveSelf(false);
            itemNeedView.gameObject.CheckActiveSelf(false);
            btn.gameObject.CheckActiveSelf(true);

            DeactivateAllShadow();
        }

        void ActivateShadow(int index)
        {
            var point = slotPoints[index];
            var shadowSpriteRenderer = point.Find("Shadow")?.GetComponent<SpriteRenderer>();
            if (shadowSpriteRenderer != null)
            {
                shadowSpriteRenderer.gameObject.CheckActiveSelf(true);
                var color = shadowSpriteRenderer.color;
                var a = color.a;
                color.a = 0;
                shadowSpriteRenderer.color = color;
                shadowSpriteRenderer.DOFade(a, 0.2f).SetDelay(0.3f);
            }
        }

        void DeactivateAllShadow()
        {
            for (int i = 0; i < slotPoints.Length; i++)
            {
                var point = slotPoints[i];
                var shadowSpriteRenderer = point.Find("Shadow")?.GetComponent<SpriteRenderer>();
                if (shadowSpriteRenderer != null)
                {
                    shadowSpriteRenderer.gameObject.CheckActiveSelf(false);
                }
            }
        }

        public void TryEnable()
        {
            SDKListener.Instance.ShowReward(DefaultEnable);
        }

        void DefaultEnable()
        {
            Enable();
        }

        public void Enable()
        {
            var sloter = colorGroupSlotView.Sloter;
            var color = sloter.FindUrgentColor(GameController.Instance.OldItems);
            var group = sloter.OpenNewGroup(color);
            Init(group);
            boxRenderer.gameObject.CheckActiveSelf(true);
            btn.gameObject.CheckActiveSelf(false);

            if (Slot.HasEmptySlot())
            {
                sloter.AdjustNoneGroupSlot(Slot);
            }
            else
            {
                sloter.TryCompleteSlot();
            }
        }
    }
}
