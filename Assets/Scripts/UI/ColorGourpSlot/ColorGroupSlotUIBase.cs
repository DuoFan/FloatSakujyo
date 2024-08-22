using DG.Tweening;
using GameExtension;
using FloatSakujyo.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FloatSakujyo.UI
{
    public abstract class ColorGroupSlotUIBase : MonoBehaviour
    {
        protected ColorGroupSlotView colorGroupSlotView;

        [SerializeField]
        protected Transform[] slotPoints;

        public ColorGroupSlot Slot { get; protected set; }

        public void SetColorGroupSlotView(ColorGroupSlotView _colorGroupSlotView)
        {
            colorGroupSlotView = _colorGroupSlotView;
        }

        public virtual void Init(ColorGroupSlot colorGourpSlot)
        {
            Slot = colorGourpSlot;
            Slot.OnFillItem += FillItem;
            Slot.OnRemoveItem += OnRemoveItem;
            Slot.OnAddSlot += AdaptSlotPoint;

            AdaptSlotPoint();

            colorGourpSlot.SetUseable(true);
        }

        protected virtual void AdaptSlotPoint()
        {
            for (int i = 0; i < Slot.TotalSlotCount; i++)
            {
                var slotPoint = slotPoints[i];
                slotPoint.gameObject.CheckActiveSelf(true);
            }

            for (int i = Slot.TotalSlotCount; i < slotPoints.Length; i++)
            {
                var slotPoint = slotPoints[i];
                slotPoint.gameObject.CheckActiveSelf(false);
            }
        }

        protected void ClearSlots()
        {
            foreach (var slotPoint in slotPoints)
            {
                foreach (Transform child in slotPoint)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        protected virtual IEnumerator FillItem(Item item, int index)
        {
            yield return null;

            var time = 0.3f;

            var point = slotPoints[index];

            item.transform.SetParent(point);

            var sequence = DOTween.Sequence();

            sequence.Join(item.transform.DOLocalRotateQuaternion(Quaternion.Euler(0, 10, 0), time));
            sequence.Join(item.transform.DOLocalMove(Vector3.up, time));

            var scale = GetWorldScale(colorGroupSlotView.transform) * 1.05f;
            var scale2 = GetWorldScale(point);
            scale = new Vector3(scale.x / scale2.x, scale.y / scale2.y, scale.z / scale2.z);

            sequence.Join(item.transform.DOScale(scale, time));

            time = 0.1f;

            sequence.Append(item.transform.DOLocalMove(Vector3.up, time));

            time = 0.2f;
            sequence.Join(item.transform.DORotateQuaternion(Quaternion.identity, time));

            yield return new WaitForSecondsRealtime(time / 2f);

            yield return sequence.WaitForCompletion();
        }

        protected virtual void OnRemoveItem(Item item, int index)
        {

        }

        public virtual IEnumerator OnCompleted()
        {
            Slot.SetUseable(false);
            yield break;
        }

        protected Vector3 GetWorldScale(Transform transform)
        {
            Vector3 worldScale = transform.localScale;
            Transform parent = transform.parent;
            while (parent != null)
            {
                worldScale = Vector3.Scale(worldScale, parent.localScale);
                parent = parent.parent;
            }
            return worldScale;
        }

        public virtual void Disable()
        {
            Slot = null;
        }
    }
}
