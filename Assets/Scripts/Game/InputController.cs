using FloatSakujyo.Game;
using FloatSakujyo.Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FloatSakujyo.Game
{
    public class InputController : InputControllerBase
    {

        protected InputStrategyBase inputStrategy;
        public void Init()
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                inputStrategy = new Windows_InputStrategy(this);
            }
            else if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                inputStrategy = new WebGL_InputStrategy(this);
            }
            else
            {
                inputStrategy = null;
            }
        }

        public override Item TryCatchItem()
        {
            return inputStrategy?.TryCatchItem();
        }

        protected abstract class InputStrategyBase
        {
            protected InputController inputController;
            protected Vector2 lastMousePosition;
            public InputStrategyBase(InputController inputController)
            {
                this.inputController = inputController;
            }

            public abstract Item TryCatchItem();
        }

        class Windows_InputStrategy : InputStrategyBase
        {
            public Windows_InputStrategy(InputController inputController) : base(inputController)
            {

            }

            public override Item TryCatchItem()
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        return null;
                    }
                    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out var hit, 1000, Const.ITEM_LAYER_MASK | Const.WATER_LAYER_MASK, QueryTriggerInteraction.Collide))
                    {
                        return hit.collider.GetComponent<Item>();
                    }
                }
                return null;
            }
        }
        class WebGL_InputStrategy : InputStrategyBase
        {
            bool isDragging;
            private Vector2 initialTouchPosition;
            private const float dragThreshold = 10f; // 拖动阈值，单位像素
            public WebGL_InputStrategy(InputController inputController) : base(inputController)
            {

            }

            public override Item TryCatchItem()
            {
                if (Input.touchCount != 1)
                {
                    return null;
                }

                var touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        initialTouchPosition = touch.position;
                        isDragging = false;
                        break;

                    case TouchPhase.Moved:
                        if (Vector2.Distance(initialTouchPosition, touch.position) > dragThreshold)
                        {
                            isDragging = true;
                        }
                        break;

                    case TouchPhase.Ended:

                        if (!isDragging)
                        {
                            var ray = Camera.main.ScreenPointToRay(touch.position);
                            if (Physics.Raycast(ray, out var hit, 1000, Const.ITEM_LAYER_MASK | Const.WATER_LAYER_MASK, QueryTriggerInteraction.Collide))
                            {
                                return hit.collider.GetComponent<Item>();
                            }
                        }
                        break;
                }

                return null;
            }
        }
    }
}
