using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameExtension
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanel : MonoBehaviour
    {
        public bool canBackByMask = true;
        public bool canAttachMask = true;
        public bool isMaskTransparent = false;
        public PanelEventHandler EventHandler { get; set; }
        public CanvasGroup CanvasGroup
        {
            get
            {
                if(canvasGroup == null)
                {
                    canvasGroup = GetComponent<CanvasGroup>();
                }
                return canvasGroup;
            }
        }
        protected CanvasGroup canvasGroup;

        [SerializeField]
        protected Button closeBtn;

        public void DontBlockRaycast()
        {
            CanvasGroup.blocksRaycasts = false;
        }
        public void BlockRaycast()
        {
            CanvasGroup.blocksRaycasts = true;
        }

        public virtual void OnShowStart() { }
        public virtual void OnShowEnd() { }
        public virtual void OnCloseStart() { }
        public virtual void OnCloseEnd() { }
        public virtual void OnOverlay() { }
        public virtual void OnResume() { }
    };
}
