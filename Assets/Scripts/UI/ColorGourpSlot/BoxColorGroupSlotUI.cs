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
                if(item != null)
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

            yield return base.FillItem(item, index);
        }

        public override IEnumerator OnCompleted()
        {
            //提前获取自身索引，因为在yield return base.OnCompleted()之后，自身会从slotUIs中移除
            var selfIndex = Array.IndexOf(colorGroupSlotView.SlotUIs, this);

            yield return base.OnCompleted();

            Slot.OnFillItem -= FillItem;
            Slot = null;

            animator.Play("close");

           // var goldStart = colorGroupSlotView.SlotGoldStarts[selfIndex];

            //GameUIManager.Instance.PlayFlyGold(goldStart);

            //盖子动画时间
            yield return new WaitForSecondsRealtime(0.25f);

            for (int i = 0; i < slotPoints.Length; i++)
            {
                GameObject.Destroy(slotPoints[i].gameObject);
            }

            //yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f);

            //yield return transform.DOMove(transform.position + Vector3.up * 30, 0.2f).WaitForCompletion();

            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);

            seal.gameObject.CheckActiveSelf(true);

            yield return new WaitUntil(() => seal.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);

            gameObject.CheckActiveSelf(false);

            yield return colorGroupSlotView.PlayItemNeedViewExit(this, selfIndex);

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
