using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace GameExtension
{
    public class Magnifier : MonoBehaviour
    {
        public float MaxScale { get; set; }
        public float MinScale { get; set; }
        public float ScaleCoex { get; set; }

        public float Scale { get; private set; }

        Vector2 lastRadius = Vector3.zero;

        Coroutine magnifing;

        public event Action<float> OnScale;

        public void Init(float scale)
        {
            Scale = scale;
        }

        public void StartMagnifing()
        {
            if (magnifing == null)
            {
                magnifing = StartCoroutine(Magnifing());
            }
        }

        public void StopMagnifing()
        {
            if (magnifing != null)
            {
                StopCoroutine(magnifing);
                magnifing = null;
            }
        }

        IEnumerator Magnifing()
        {
            lastRadius = Vector2.zero;
            while (true)
            {
                yield return null;
                if (Input.touchCount == 2)
                {
                    var touch0 = Input.GetTouch(0);
                    var touch1 = Input.GetTouch(1);
                    if (touch0.phase == TouchPhase.Moved && touch1.phase == TouchPhase.Moved)
                    {
                        if (lastRadius != Vector2.zero)
                        {
                            //单位是像素，除以100是为了缩小缩放的幅度
                            var delta = ((touch1.position - touch0.position).magnitude - lastRadius.magnitude) / 100;
                            SetScale(delta);
                        }
                        lastRadius = (touch1.position - touch0.position);
                    }
                }
                else if (Input.mouseScrollDelta.SqrMagnitude() > 0)
                {
                    SetScale(Input.mouseScrollDelta.y);
                }
                else
                {
                    lastRadius = Vector2.zero;
                }
            }
        }

        void SetScale(float delta)
        {
            //用减去，否则方向错误
            Scale -= delta * ScaleCoex;
            Scale = Mathf.Clamp(Scale, MinScale, MaxScale);
            OnScale?.Invoke(Scale);
        }
    }
}
