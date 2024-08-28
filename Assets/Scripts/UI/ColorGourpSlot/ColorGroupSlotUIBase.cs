using DG.Tweening;
using GameExtension;
using FloatSakujyo.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FloatSakujyo.Audio;

namespace FloatSakujyo.UI
{
    public abstract class ColorGroupSlotUIBase : MonoBehaviour
    {
        protected ColorGroupSlotView colorGroupSlotView;

        [SerializeField]
        protected Vector3 fillLocalPosition;
        [SerializeField]
        protected Vector3 fillLocalEulerAngle;

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

            item.ShowTrail();

            var sequence = DOTween.Sequence();

            sequence.Join(item.transform.DOLocalRotate(Quaternion.Euler(0, 10, 0) * fillLocalEulerAngle, time));
            sequence.Join(item.transform.DOLocalMove(Vector3.up, time));
            sequence.Join(item.transform.DOScale(Vector3.one, time));

            time = 0.1f;

            sequence.Append(item.transform.DOLocalMove(fillLocalPosition, time));

            time = 0.2f;
            sequence.Join(item.transform.DOLocalRotate(fillLocalEulerAngle, time));

            yield return new WaitForSecondsRealtime(time / 2f);

            AudioManager.Instance.PlayPut();

            yield return sequence.WaitForCompletion();

            item.HideTrail();
        }

        protected virtual void OnRemoveItem(Item item, int index)
        {

        }

        public virtual IEnumerator OnCompleted()
        {
            AudioManager.Instance.PlayDone();
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
