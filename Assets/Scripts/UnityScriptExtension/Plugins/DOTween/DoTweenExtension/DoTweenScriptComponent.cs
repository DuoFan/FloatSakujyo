using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace GameExtension
{
    public class DoTweenScriptComponent : MonoBehaviour
    {
        private enum Type
        {
            Position = 0,
            LocalPosition = 1,
            Alpha = 2,
            MoveY = 3,
            MoveX = 4,
            DelayCallback = 5,
            LocalMoveY = 6,
            LocalMoveX = 7,
            Scale = 8,
            ScaleY = 9,
            ScaleX = 10,
            Rotation = 11,
            LocalRotation = 12
        }

        private enum Loop
        {
            Once = 0,
            Restart = 1,
            Yoyo = 2
        }

        [Serializable]
        private class DoTweenInfo
        {
            public Type TweenType;

            public float Value;

            public Vector3 To;

            public float Duration;

            public float Delay;

            public Ease EaseType;

            public bool customGraph;

            public AnimationCurve customCurve;

            public UnityEvent Callback;
        }

        [SerializeField]
        private List<DoTweenInfo> Tweens;

        [SerializeField]
        private Loop LoopType;

        [SerializeField]
        private bool AutoPlay;

        [SerializeField]
        private bool OnEnablePlay;

        private Tween tween;

        private void Awake()
        {
            if (AutoPlay)
            {
                Play();
            }
        }

        private void OnDestroy()
        {
            Kill();
        }

        private void OnEnable()
        {
            if (OnEnablePlay)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            Kill();
        }


        public void Play()
        {
#if UNITY_EDITOR
            Kill();
#else
            if(tween != null)
            {
                return;
            }
#endif

            if (Tweens.Count == 1)
            {
                tween = GetTween(Tweens[0]);
            }
            else if(Tweens.Count > 1)
            {
                var seq = DOTween.Sequence();

                for (int i = 0; i < Tweens.Count; i++)
                {
                    seq.Append(GetTween(Tweens[i]));
                }
                tween = seq;
            }

            if(tween != null)
            {
                switch (LoopType)
                {
                    case Loop.Restart:
                        tween.SetLoops(-1, DG.Tweening.LoopType.Restart);
                        break;
                    case Loop.Yoyo:
                        tween.SetLoops(-1, DG.Tweening.LoopType.Yoyo);
                        break;
                }
            }
        }

        Tween GetTween(DoTweenInfo doTweenInfo)
        {
            Tween tween = null;
            switch (doTweenInfo.TweenType)
            {
                case Type.Position:
                    tween = transform.DOMove(doTweenInfo.To, doTweenInfo.Duration);
                    break;
                case Type.Rotation:
                    tween = transform.DORotate(doTweenInfo.To, doTweenInfo.Duration);
                    break;
                case Type.LocalPosition:
                    tween = transform.DOLocalMove(doTweenInfo.To, doTweenInfo.Duration);
                    break;
                case Type.Alpha:
                    var canvasGroup = GetComponent<CanvasGroup>();
                    if (canvasGroup != null)
                    {
                        tween = canvasGroup.DOFade(doTweenInfo.Value, doTweenInfo.Duration);
                        goto Break;
                    }

                    var image = GetComponent<UnityEngine.UI.Image>();
                    if (image != null)
                    {
                        tween = image.DOFade(doTweenInfo.Value, doTweenInfo.Duration);
                        goto Break;
                    }

                    var spriteRenderer = GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        tween = spriteRenderer.DOFade(doTweenInfo.Value, doTweenInfo.Duration);
                        goto Break;
                    }

                Break:
                    break;
                case Type.MoveY:
                    tween = transform.DOMoveY(doTweenInfo.Value, doTweenInfo.Duration);
                    break;
                case Type.MoveX:
                    tween = transform.DOMoveX(doTweenInfo.Value, doTweenInfo.Duration);
                    break;
                case Type.DelayCallback:
                    //seq.AppendCallback(doTweenInfo.Callback.Invoke);
                    break;
                case Type.LocalMoveY:
                    tween = transform.DOLocalMoveY(doTweenInfo.Value, doTweenInfo.Duration);
                    break;
                case Type.LocalMoveX:
                    tween = transform.DOLocalMoveX(doTweenInfo.Value, doTweenInfo.Duration);
                    break;
                case Type.Scale:
                    tween = transform.DOScale(doTweenInfo.To, doTweenInfo.Duration);
                    break;
            }

            if (doTweenInfo.TweenType != Type.DelayCallback)
            {
                tween.SetDelay(doTweenInfo.Delay);

                if (doTweenInfo.customGraph)
                {
                    tween.SetEase(doTweenInfo.customCurve);
                }
                else
                {
                    tween.SetEase(doTweenInfo.EaseType);
                }

                tween.OnComplete(doTweenInfo.Callback.Invoke);
            }

            return tween;
        }

        public void Kill()
        {
            if(tween != null)
            {
                tween.Pause();
                tween.Kill();
                tween = null;
            }
        }
    }
}
