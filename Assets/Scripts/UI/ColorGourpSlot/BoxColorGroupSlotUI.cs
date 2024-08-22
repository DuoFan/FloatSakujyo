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

namespace FloatSakujyo.UI
{
    public class BoxColorGroupSlotUI : ColorGroupSlotUIBase
    {
        [SerializeField]
        MeshRenderer boxRenderer;
        [SerializeField]
        MeshRenderer coverRenderer;
        [SerializeField]
        Button btn;

        [SerializeField]
        Animator animator;

        public ItemColor ItemColor => Slot.ItemColor;

        public bool IsEnable => Slot != null;

        public void Awake()
        {
            //btn.onClick.AddListener(TryEnable);
        }

        public override void Init(ColorGroupSlot colorGourpSlot)
        {
            base.Init(colorGourpSlot);

            /*var colorConfig = ItemColorConfigDataManager.Instance.GetConfigData(ItemColor);

            var material = new Material(boxRenderer.material);
            material.mainTexture = colorConfig.BoxTexture;
            boxRenderer.material = material;

            material = new Material(coverRenderer.material);
            material.mainTexture = colorConfig.CoverTexture;
            coverRenderer.material = material;*/

            for (int i = 0; i < Slot.Items.Length; i++)
            {
                var item = Slot.Items[i];
                if(item != null)
                {
                    var point = slotPoints[i];
                    item.transform.SetParent(point);

                    item.transform.localPosition = Vector3.zero;

                    var scale = GetWorldScale(colorGroupSlotView.transform) * 1.05f;
                    var scale2 = GetWorldScale(point);
                    scale = new Vector3(scale.x / scale2.x, scale.y / scale2.y, scale.z / scale2.z);
                    item.transform.localScale = scale;

                    item.transform.rotation = Quaternion.Euler(-100, 0, 0);
                }
            }
        }


        public override IEnumerator OnCompleted()
        {
            //提前获取自身索引，因为在yield return base.OnCompleted()之后，自身会从slotUIs中移除
            var selfIndex = Array.IndexOf(colorGroupSlotView.SlotUIs, this);

            yield return base.OnCompleted();

            Slot.OnFillItem -= FillItem;
            Slot = null;

            //animator.Play("box_move");

           // var goldStart = colorGroupSlotView.SlotGoldStarts[selfIndex];

            //GameUIManager.Instance.PlayFlyGold(goldStart);

            //盖子动画时间
            yield return new WaitForSecondsRealtime(0.04f);

            for (int i = 0; i < slotPoints.Length; i++)
            {
                GameObject.Destroy(slotPoints[i].gameObject);
            }

            //yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f);

            yield return transform.DOMove(transform.position + Vector3.up * 30, 0.2f).WaitForCompletion();

            Destroy(gameObject);
        }

        public override void Disable()
        {
            base.Disable();
            boxRenderer.gameObject.CheckActiveSelf(false);
            //btn.gameObject.CheckActiveSelf(true);
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
            var color = sloter.FindUrgentColor(GameController.Instance.LevelEntity.OldItems);
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
