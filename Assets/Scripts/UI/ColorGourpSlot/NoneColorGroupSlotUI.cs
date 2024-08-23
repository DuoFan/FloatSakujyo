using GameExtension;
using FloatSakujyo.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

namespace FloatSakujyo.UI
{
    public class NoneColorGroupSlotUI : ColorGroupSlotUIBase
    {
        [SerializeField]
        Image warningImg;
        float groupSlotWidth;

        public ItemColor ItemColor => Slot.ItemColor;

        public void SetGroupSlotWidth(float width)
        {
            groupSlotWidth = width;
        }

        public override void Init(ColorGroupSlot colorGourpSlot)
        {
            base.Init(colorGourpSlot);
            //warningImg.gameObject.CheckActiveSelf(false);
        }

        protected override void AdaptSlotPoint()
        {
            base.AdaptSlotPoint();

            int activatedSlotPointCount = 0;
            for (int i = 0; i < slotPoints.Length; i++)
            {
                if (slotPoints[i].gameObject.activeSelf)
                {
                    activatedSlotPointCount++;
                }
            }

            var start = -groupSlotWidth / 2f;
            var delta = groupSlotWidth / (activatedSlotPointCount - 1);
            for (int i = 0; i < activatedSlotPointCount; i++)
            {
                var slotPoint = slotPoints[i];
                slotPoint.transform.localPosition = Vector3.right * (start + i * delta);
            }
        }

        protected override IEnumerator FillItem(Item item, int index)
        {
            yield return base.FillItem(item, index);

            //倒数第二个洞
            /*if (Slot.EmptySlotCount == 1 && warningImg != null)
            {
                var lastestSlotIndex = 0;
                for (int i = 0; i < Slot.Items.Length; i++)
                {
                    if (Slot.Items[i] == null)
                    {
                        lastestSlotIndex = i;
                        break;
                    }
                }

                var lastestSlot = slotPoints[lastestSlotIndex];
                warningImg.gameObject.CheckActiveSelf(true);
                warningImg.transform.position = lastestSlot.transform.TransformPoint(new Vector3(0.1f, 0.7f));
            }*/
        }

        protected override void OnRemoveItem(Item item, int index)
        {
            base.OnRemoveItem(item, index);
            //warningImg.gameObject.CheckActiveSelf(false);
        }
    }
}
