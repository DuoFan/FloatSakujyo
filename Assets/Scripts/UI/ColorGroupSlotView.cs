using DG.Tweening;
using GameExtension;
using FloatSakujyo.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace FloatSakujyo.UI
{
    public class ColorGroupSlotView : MonoBehaviour
    {
        [SerializeField]
        ColorGroupSlotUIBase slotUIPrototype;

        [SerializeField]
        ColorGroupSlotUIBase[] slotUIs;
        public ColorGroupSlotUIBase[] SlotUIs => slotUIs;

        [SerializeField]
        Transform[] slotGoldStart;
        public Transform[] SlotGoldStarts => slotGoldStart;


        Vector3[] slotUILocalPoses;

        public ColorGroupSloter Sloter { get; private set; }

        [SerializeField]
        bool[] isMoveRight;
        public bool[] IsMoveRight => isMoveRight;

        bool[] isCompleting;

        public void InitSlotUIPosition(float width)
        {
            slotUILocalPoses = new Vector3[slotUIs.Length];

            var startX = -width / 2f;
            var delta = width / (slotUIs.Length - 2f);
            for (int i = 1; i < slotUIs.Length; i++)
            {
                slotUIs[i].transform.localPosition = new Vector3(startX + (i - 1) * delta, 2, 2.7f);
            }

            for (int i = 0; i < slotUIs.Length; i++)
            {
                slotUILocalPoses[i] = slotUIs[i].transform.localPosition;
                slotUIs[i].SetColorGroupSlotView(this);
            }
        }

        public void InitNoneColorGroupSlotUIPosition(float width)
        {
            (slotUIs[0] as NoneColorGroupSlotUI).SetGroupSlotWidth(width);
        }

        public void Init(ColorGroupSloter _sloter)
        {
            Sloter = _sloter;

            for (int i = 0; i < SlotUIs.Length; i++)
            {
                if (SlotUIs[i] == null)
                {
                    CreateSlotForIndex(i);
                }
            }


            for (int i = 0; i < Sloter.ColorGroupSlots.Length; i++)
            {
                if (Sloter.ColorGroupSlots[i] == null)
                {
                    continue;
                }

                SlotUIs[i].Init(Sloter.ColorGroupSlots[i]);
            }

            for (int i = Sloter.ColorGroupSlots.Length; i < slotUIs.Length; i++)
            {
                if (slotUIs[i] != null)
                {
                    slotUIs[i].Disable();
                }
            }

            isCompleting = new bool[slotUIs.Length];

            _sloter.OnSlotCompleted += OnSlotCompleted;
            _sloter.OnSlotGenerated += OnSlotGenerated;
        }

        void CreateSlotForIndex(int index)
        {
            slotUIs[index] = Instantiate(slotUIPrototype, transform);
            slotUIs[index].transform.localPosition = slotUILocalPoses[index];
            slotUIs[index].gameObject.CheckActiveSelf(true);
            slotUIs[index].SetColorGroupSlotView(this);
        }

        private void OnSlotCompleted(ColorGroupSlot slot)
        {
            for (int i = 0; i < slotUIs.Length; i++)
            {
                if (slotUIs[i] != null && slotUIs[i].Slot == slot)
                {
                    StartCoroutine(WaitForSlotUICompleted(slotUIs[i], i));
                    slotUIs[i] = null;
                    return;
                }
            }
        }


        IEnumerator WaitForSlotUICompleted(ColorGroupSlotUIBase slotUI,int index)
        {
            isCompleting[index] = true;

            yield return slotUI.OnCompleted();

            isCompleting[index] = false;
        }

        private void OnSlotGenerated(ColorGroupSlot slot)
        {
            if (slot.HasEmptySlot())
            {
                for (int i = 0; i < slotUIs.Length; i++)
                {
                    if (slotUIs[i] == null)
                    {
                        Sloter.AddGeneratingSlotGroup();
                        CreateSlotForIndex(i);
                        if (IsMoveRight[i])
                        {
                            slotUIs[i].transform.localPosition = slotUILocalPoses[1] + Vector3.left * 5;
                        }
                        else
                        {
                            slotUIs[i].transform.localPosition = slotUILocalPoses[slotUILocalPoses.Length - 1] + Vector3.right * 5;
                        }
                        slot.SetUseable(false);
                        StartCoroutine(WaitForSlotUIEntered(slotUIs[i], slot, slotUILocalPoses[i]));
                        return;
                    }
                }

                var error = $"生成新组时找不到空余的槽位";
                GameExtension.Logger.Error(error);
                throw new Exception(error);
            }
            else
            {
                Sloter.TryCompleteSlot();
            }
        }

        IEnumerator WaitForSlotUIEntered(ColorGroupSlotUIBase slotUI, ColorGroupSlot slot, Vector3 pos)
        {
            var index = Array.IndexOf(slotUIs, slotUI);
            yield return new WaitUntil(() => !isCompleting[index]);

            slotUI.Init(slot);

            slot.SetUseable(false);

            yield return slotUI.transform.DOLocalMove(pos, 0.5f).SetDelay(0.5f).WaitForCompletion();
            slot.SetUseable(true);
            if (slot.HasEmptySlot())
            {
                Sloter.AdjustNoneGroupSlot(slot);
            }
            else
            {
                Sloter.TryCompleteSlot();
            }
            Sloter.RemoveCompletingSlotGroup();
        }

        public IEnumerator PlayItemNeedViewEnter(BoxColorGroupSlotUI boxColorGroupSlotUI)
        {
            var itemNeedView = boxColorGroupSlotUI.ItemNeedView;
            itemNeedView.gameObject.CheckActiveSelf(true);

            itemNeedView.Avatar.sprite = CatSpriteManager.Instance.GetRandomCatSprite();
            var slotUIIndex = Array.IndexOf(slotUIs, boxColorGroupSlotUI);
            var canvas = GameController.Instance.ColorGroupSlotViewCanvas;

            itemNeedView.transform.SetParent(canvas.transform);
            itemNeedView.transform.SetSiblingIndex(2);

            itemNeedView.transform.localScale = Vector3.one;
            itemNeedView.transform.localRotation = Quaternion.identity;

            bool isMoveRight = this.isMoveRight[slotUIIndex];
            var pos = isMoveRight ? slotUILocalPoses[1] + Vector3.left * 5 : slotUILocalPoses[slotUILocalPoses.Length - 1] + Vector3.right * 5;
            pos = transform.TransformPoint(pos);
            pos = canvas.transform.InverseTransformPoint(pos);
            pos.y += itemNeedView.Avatar.rectTransform.rect.size.y * 0.8f;
            pos.z += 500;

            var itemNeedViewRectTrans = itemNeedView.transform as RectTransform;
            itemNeedViewRectTrans.anchoredPosition3D = pos;

            pos = slotUILocalPoses[slotUIIndex];
            pos = transform.TransformPoint(pos);
            pos = canvas.transform.InverseTransformPoint(pos);
            pos.y += itemNeedView.Avatar.rectTransform.rect.size.y * 0.8f;
            pos.z += 500;

            yield return itemNeedViewRectTrans.DOAnchorPos3D(pos, 1f).SetEase(Ease.OutBack).WaitForCompletion();

            itemNeedViewRectTrans.anchoredPosition3D = pos;

            var itemColor = boxColorGroupSlotUI.Slot.ItemColor;
            var itemManager = ItemColorConfigDataManager.Instance;
            var item = GameObject.Instantiate(itemManager.GetConfigData(itemColor).Item, itemNeedView.ItemPlace);
            //删除Item组件避免被点击
            GameObject.Destroy(item);

            itemNeedView.View.CheckActiveSelf(true);

            yield break;
        }

        public IEnumerator PlayItemNeedViewExit(BoxColorGroupSlotUI boxColorGroupSlotUI, int slotIndex)
        {
            var itemNeedView = boxColorGroupSlotUI.ItemNeedView;

            itemNeedView.View.CheckActiveSelf(false);

            var slotUIIndex = slotIndex;
            var canvas = GameController.Instance.ColorGroupSlotViewCanvas;

            bool isMoveRight = this.isMoveRight[slotUIIndex];
            var pos = isMoveRight ? slotUILocalPoses[1] + Vector3.left * 5 : slotUILocalPoses[slotUILocalPoses.Length - 1] + Vector3.right * 5;
            pos = transform.TransformPoint(pos);
            pos = canvas.transform.InverseTransformPoint(pos);
            pos.y += itemNeedView.Avatar.rectTransform.rect.size.y * 0.8f;
            pos.z += 500;

            var itemNeedViewRectTrans = itemNeedView.transform as RectTransform;

            itemNeedViewRectTrans.SetSiblingIndex(2);

            yield return itemNeedViewRectTrans.DOAnchorPos3D(pos, 1f).WaitForCompletion();
        }
    }
}

