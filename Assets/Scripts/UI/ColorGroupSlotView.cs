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


        public void Awake()
        {
            slotUILocalPoses = new Vector3[slotUIs.Length];
            for (int i = 0; i < slotUIs.Length; i++)
            {
                slotUILocalPoses[i] = slotUIs[i].transform.localPosition;
                slotUIs[i].SetColorGroupSlotView(this);
            }
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
                    StartCoroutine(slotUIs[i].OnCompleted());
                    slotUIs[i] = null;
                    return;
                }
            }
        }
        private void OnSlotGenerated(ColorGroupSlot slot)
        {
            for (int i = 0; i < slotUIs.Length; i++)
            {
                if (slotUIs[i] == null)
                {
                    Sloter.AddGeneratingSlotGroup();
                    CreateSlotForIndex(i);
                    slotUIs[i].transform.localPosition = slotUILocalPoses[1] + Vector3.left * 5;
                    slotUIs[i].Init(slot);
                    slot.SetUseable(false);
                    StartCoroutine(WaitForSlotUIEntered(slotUIs[i], slot, slotUILocalPoses[i]));
                    return;
                }
            }

            var error = $"生成新组时找不到空余的槽位";
            GameExtension.Logger.Error(error);
            throw new Exception(error);
        }

        IEnumerator WaitForSlotUIEntered(ColorGroupSlotUIBase slotUI, ColorGroupSlot slot, Vector3 pos)
        {
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
    }
}

