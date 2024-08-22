using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;

namespace GameExtension
{
    public class UIManager : MonoBehaviour
    {
        const float DEFAULT_PANEL_TIME = 0.25f;

        public static UIManager Instance { get; protected set; }
        [SerializeField] protected Image panelMask;
        [SerializeField] protected Image transparentMask;

        protected Image PanelMask
        {
            get
            {
                if (panelMask == null)
                {
                    var obj = new GameObject("PanelMask");
                    obj.transform.SetParent(transform);
                    panelMask = obj.AddComponent<Image>();
                    panelMask.AddComponent<Button>().onClick.AddListener(TryClosePeekPanelByMask);
                    var btn = panelMask.GetComponent<Button>();
                    panelMask.rectTransform.SetAnchor(ScriptExtension.AnchorPresets.StretchAll);
                    panelMask.rectTransform.sizeDelta = Vector2.zero;
                    panelMask.color = new Color(0, 0, 0, 0.5f);
                    panelMask.enabled = false;
                }

                return panelMask;
            }
            set { panelMask = value; }
        }

        protected Image TransparentMask
        {
            get
            {
                if (transparentMask == null)
                {
                    var obj = new GameObject("TransparentMask");
                    obj.transform.SetParent(transform);
                    transparentMask = obj.AddComponent<Image>();
                    transparentMask.AddComponent<Button>().onClick.AddListener(TryClosePeekPanelByMask);
                    var btn = transparentMask.GetComponent<Button>();
                    transparentMask.rectTransform.SetAnchor(ScriptExtension.AnchorPresets.StretchAll);
                    transparentMask.rectTransform.sizeDelta = Vector2.zero;
                    transparentMask.color = new Color(0, 0, 0, 0);
                    transparentMask.enabled = false;
                }

                return transparentMask;
            }
            set { panelMask = value; }
        }

        protected List<UIPanel> panels = new List<UIPanel>();
        Dictionary<Type, UIInitializer> initializers;

        protected virtual void Awake()
        {
            Instance = this;

            if (panelMask == null)
            {
                PanelMask.name = "PanelMask";
            }
            else
            {
                var btn = panelMask.GetComponent<Button>();
                if (btn == null)
                {
                    btn = panelMask.gameObject.AddComponent<Button>();
                }
                btn.onClick.AddListener(TryClosePeekPanelByMask);
            }
            if (transparentMask == null)
            {
                TransparentMask.name = "TransparentMask";
            }
            else
            {
                var btn = transparentMask.GetComponent<Button>();
                if (btn == null)
                {
                    btn = transparentMask.gameObject.AddComponent<Button>();
                }
                btn.onClick.AddListener(TryClosePeekPanelByMask);
            }

            RegisteChildrenUIInitializer(transform);
        }

        public void EnableRaycaster()
        {
            GetComponent<GraphicRaycaster>().enabled = true;
        }

        public void DisableRaycaster()
        {
            GetComponent<GraphicRaycaster>().enabled = false;
        }

        public void RegisteChildrenUIInitializer(Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var uIInitializer = transform.GetChild(i).GetComponent<UIInitializer>();
                if (uIInitializer != null)
                {
                    RegisteUIInitializer(uIInitializer);
                }
            }
        }

        public void RegisteUIInitializer(UIInitializer uIInitializer)
        {
            if (uIInitializer != null)
            {
                if (initializers == null)
                {
                    initializers = new Dictionary<Type, UIInitializer>();
                }
                if (uIInitializer.Prototype == null)
                {
                    var error = $"UIInitializer{uIInitializer.name}未设置原型";
                    GameExtension.Logger.Error(error);
                    throw new Exception(error);
                }
                var initUI = uIInitializer.Prototype.GetComponent<IInitUI>();
                if (initUI != null)
                {
                    initializers.Add(initUI.GetType(), uIInitializer);
                    return;
                }

                var uiPanel = uIInitializer.Prototype.GetComponent<UIPanel>();
                if (uiPanel != null)
                {
                    initializers.Add(uiPanel.GetType(), uIInitializer);
                    return;
                }
                var error2 = $"UIInitializer:{uIInitializer.name}的原型未实现IInitUI或UIPanel";
                GameExtension.Logger.Error(error2);
                throw new Exception(error2);
            }
        }

        public T GetUI<T>(ref T panelHolder)
        {
            if (panelHolder == null)
            {
                var type = typeof(T);
                if (!initializers.TryGetValue(type, out var initializer))
                {
                    var error = $"未找到{type}的UIInitializer";
                    GameExtension.Logger.Error(error);
                    throw new Exception(error);
                }

                panelHolder = initializer.InitUI().GetComponent<T>();

                initializers.Remove(type);

                if(initializers.Count == 0)
                {
                    initializers.TrimExcess();
                    initializers = null;
                }
            }

            return panelHolder;
        }

        public void OpenPanel<T>(ref T panel, float duration = DEFAULT_PANEL_TIME) where T : UIPanel
        {
            OpenPanel(GetUI(ref panel));
        }
        public void ClosePanel<T>(ref T panel, float duration = DEFAULT_PANEL_TIME) where T : UIPanel
        {
            ClosePanel(GetUI(ref panel), duration);
        }


        public virtual bool OpenPanel(UIPanel panel, float duration = DEFAULT_PANEL_TIME,
            PanelEventHandler panelEvent = null)
        {
            if (panel == null)
            {
                return false;
            }

            if (!panels.Contains(panel))
            {
                panels.Add(panel);
            }

            if (panels.Count > 0)
            {
                var lastPanel = panels[panels.Count - 1];
                lastPanel.EventHandler?.onOverlay?.Invoke();
                lastPanel.OnOverlay();
            }

            panel.OnShowStart();
            panel.EventHandler = panelEvent;
            panel.EventHandler?.onShow?.onStart?.Invoke();
            panel.BlockRaycast();
            var panelTween = panel.CanvasGroup.DOFade(1, duration);
            panelTween.OnComplete(() =>
            {
                panel.OnShowEnd();
                panel.EventHandler?.onShow?.onEnd?.Invoke();
                panel.CanvasGroup.interactable = true;
            });
            if (panel.canAttachMask)
            {
                AttachMaskTo(panel.transform as RectTransform, panel.isMaskTransparent);
            }

            return true;
        }

        public virtual bool ClosePanel(float duration = DEFAULT_PANEL_TIME)
        {
            if (panels.Count <= 0)
            {
                return false;
            }

            var panel = panels[panels.Count - 1];
            panels.RemoveAt(panels.Count - 1);
            panel.OnCloseStart();
            panel.EventHandler?.onClose?.onStart?.Invoke();
            panel.CanvasGroup.interactable = false;
            panel.DontBlockRaycast();
            var panelTween = panel.CanvasGroup.DOFade(0, duration).OnComplete(() => { OnPanelClosed(panel); });

            return true;
        }

        private void OnPanelClosed(UIPanel panel)
        {
            panel.OnCloseEnd();
            panel.EventHandler?.onClose?.onEnd?.Invoke();
            panel.CanvasGroup.interactable = false;
            panel.DontBlockRaycast();

            if (panels.Count > 0)
            {
                AdjustMasks(panel);
            }
            else
            {
                DisableMasks();
            }
        }

        public virtual bool ClosePanel(UIPanel panel, float duration = 0.25f)
        {
            if (panel == null)
            {
                return false;
            }

            int index = -1;

            if (panels.Count > 0 && panel == panels[panels.Count - 1])
            {
                return ClosePanel(duration);
            }
            else if ((index = panels.IndexOf(panel)) >= 0)
            {
                panels.RemoveAt(index);

                panel.EventHandler?.onClose?.onStart?.Invoke();
                panel.OnCloseStart();
                panel.CanvasGroup.interactable = false;
                panel.DontBlockRaycast();
                var panelTween = panel.CanvasGroup.DOFade(0, duration).OnComplete(() => { OnPanelClosed(panel); });
                return true;
            }

            return false;
        }

        protected void AttachMaskTo(RectTransform target, bool isTransparent)
        {
            var panelMask = isTransparent ? TransparentMask : PanelMask;

            if (panelMask.transform.parent != target.parent)
            {
                panelMask.transform.SetParent(target.parent);
                //(panelMask.transform as RectTransform).anchoredPosition = Vector2.zero;
            }

            var targetIndex = target.GetSiblingIndex();
            var maskIndex = panelMask.transform.GetSiblingIndex();
            if (maskIndex != targetIndex - 1)
            {
                if (targetIndex == target.transform.childCount - 1)
                {
                    panelMask.transform.SetSiblingIndex(targetIndex - 1);
                }
                else if (maskIndex < targetIndex)
                {
                    panelMask.transform.SetSiblingIndex(targetIndex - 1);
                }
                else
                {
                    panelMask.transform.SetSiblingIndex(targetIndex);
                }
            }

            StartCoroutine(AdjustPanelMask(panelMask));
            panelMask.enabled = true;
        }

        IEnumerator AdjustPanelMask(Image panelMask)
        {
            yield return null;
            //panelMask.transform.localPosition = Vector3.zero;
            panelMask.transform.localScale = Vector3.one;
            if(panelMask.enabled != panels.Count > 0)
            {
                panelMask.enabled = panels.Count > 0;
            }
        }

        public void RefreshContentFitters(params ContentSizeFitter[] contentSizeFitters)
        {
            for (int i = 0; i < contentSizeFitters.Length; i++)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentSizeFitters[i].transform as RectTransform);
            }
        }

        public void SetMaskMaterial(Material material)
        {
            panelMask.material = material;
        }

        void TryClosePeekPanelByMask()
        {
            if (panels.Count > 0)
            {
                //关闭列尾的面板
                var panel = panels[panels.Count - 1];
                if (panel.canBackByMask)
                {
                    ClosePanel();
                }
            }
        }

        void AdjustMasks(UIPanel closedPanel)
        {
            bool isFindTransparentPanel = closedPanel.isMaskTransparent;
            Transform lastAttachMaskPanel = null;
            for (var i = panels.Count - 1; i >= 0; i--)
            {
                var curPanel = panels[i];
                if (curPanel.canAttachMask && curPanel.isMaskTransparent == isFindTransparentPanel)
                {
                    lastAttachMaskPanel = curPanel.transform;
                    break;
                }
            }

            if (lastAttachMaskPanel != null)
            {
                AttachMaskTo(lastAttachMaskPanel as RectTransform, isFindTransparentPanel);
            }
            else
            {
                if (closedPanel.isMaskTransparent)
                {
                    TransparentMask.enabled = false;
                }
                else
                {
                    PanelMask.enabled = false;
                }
            }

            var lastPanel = panels[panels.Count - 1];
            lastPanel.EventHandler?.onResume?.Invoke();
            lastPanel.OnResume();
        }

        void DisableMasks()
        {
            PanelMask.enabled = false;
            TransparentMask.enabled = false;
        }
    }
}