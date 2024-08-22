using DG.Tweening;
using GameExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameExtension
{
    public class FlyAnim : MonoBehaviour
    {
        [SerializeField]
        protected Transform flyTarget;
        [SerializeField]
        protected Transform spawnTransform;
        [SerializeField]
        protected FlyObject uiPrototype;
        protected Pool<FlyObject> uiPool;

        protected virtual void Awake()
        {
            uiPool = new Pool<FlyObject>(() =>
            {
                return GameObject.Instantiate(uiPrototype, spawnTransform);
            }, 30);
        }

        public AnimBuilder GetAnimBuilder()
        {
            var builder = new AnimBuilder();
            builder.Anim = this;
            return builder;
        }

        IEnumerator PlayMoneyAnim(AnimBuilder animBuilder)
        {
            FlyObject[] objs = new FlyObject[animBuilder.InstanceCount];

            Circle circle = new Circle(animBuilder.MaxAnimRadius, Vector2.zero);

            int initPerFrame = 1;
            if (animBuilder.InitializeInstanceInterval <= 0)
            {
                initPerFrame = animBuilder.InstanceCount;
            }
            else if (animBuilder.InitializeInstanceInterval > 0 && animBuilder.FlyingTweenInterval < 0.02f)
            {
                initPerFrame = Mathf.CeilToInt(0.02f / animBuilder.InitializeInstanceInterval);
            }
            int initLoopCount = animBuilder.InstanceCount / initPerFrame;

            int flyPerFrame = 1;
            if (animBuilder.FlyingTweenInterval <= 0)
            {
                flyPerFrame = animBuilder.InstanceCount;
            }
            else if (animBuilder.FlyingTweenInterval > 0 && animBuilder.FlyingTweenInterval < 0.02f)
            {
                flyPerFrame = Mathf.CeilToInt(0.02f / animBuilder.FlyingTweenInterval);
            }

            var spawnTransform = animBuilder.spawnTransform ?? this.spawnTransform;

            for (int i = 0; i < animBuilder.InstanceCount; i += initPerFrame)
            {
                for (int j = 0; j < initPerFrame; j++)
                {
                    int index = i + j;
                    if (index >= animBuilder.InstanceCount)
                    {
                        break;
                    }

                    var obj = uiPool.Get();
                    if (!obj.gameObject.activeSelf)
                    {
                        obj.gameObject.SetActive(true);
                    }
                    obj.OnStartFly();

                    obj.transform.localScale = Vector3.zero;
                    if (obj.transform.parent != spawnTransform)
                    {
                        obj.transform.SetParent(spawnTransform);
                    }

                    var sequence = DOTween.Sequence();

                    sequence.Append(obj.transform.DOScale(Vector3.one, 0.5f));

                    obj.transform.localPosition = Vector2.zero;
                    var randomAngle = UnityEngine.Random.Range(animBuilder.StartAngle, animBuilder.EndAngle);
                    var randomRadius = UnityEngine.Random.Range(animBuilder.MinAnimRadius, animBuilder.MaxAnimRadius);
                    var randomPos = circle.GetPosByDeg(randomAngle).normalized * randomRadius;
                    sequence.Join(obj.transform.DOLocalMove(randomPos, animBuilder.FlyingPosTime).SetEase(Ease.OutCubic));

                    float floatTime = (initLoopCount - i) * animBuilder.InitializeInstanceInterval;
                    floatTime += i * animBuilder.FlyingTweenInterval;
                    floatTime += animBuilder.WaitForFlyingTargetInterval;
                    floatTime -= animBuilder.FlyingPosTime;
                    if (floatTime > 0)
                    {
                        sequence.Append(obj.transform.DOShakePosition(floatTime, 10, 10, 90, false, false));
                    }

                    objs[index] = obj;
                }
                if (animBuilder.InitializeInstanceInterval > 0)
                {
                    if (animBuilder.InitializeInstanceInterval < 0.02f)
                    {
                        yield return new WaitForSecondsRealtime(0.02f);
                    }
                    else
                    {
                        yield return new WaitForSecondsRealtime(animBuilder.InitializeInstanceInterval);
                    }
                }
            }


            if (animBuilder.WaitForFlyingTargetInterval > 0)
            {
                yield return new WaitForSecondsRealtime(animBuilder.WaitForFlyingTargetInterval);
            }

            for (int i = 0; i < animBuilder.InstanceCount; i += flyPerFrame)
            {
                for (int j = 0; j < flyPerFrame; j++)
                {
                    int index = i + j;
                    if (index >= animBuilder.InstanceCount)
                    {
                        break;
                    }

                    var obj = objs[index];

                    Tween flyingTween = null;

                    var target = animBuilder.FlyTargetTransform ?? flyTarget;

                    flyingTween = obj.transform.DOMove(target.position, animBuilder.FlyingTargetTime).SetEase(animBuilder.Ease);

                    animBuilder.OnStartFly?.Invoke(obj);

                    flyingTween.OnComplete(() =>
                    {
                        obj.OnEndFly();
                        obj.transform.SetParent(transform);
                        uiPool.Return(obj);
                        animBuilder.OnFlyingEnd?.Invoke();
                    });
                }
                if (animBuilder.FlyingTweenInterval > 0)
                {
                    if (animBuilder.FlyingTweenInterval < 0.02f)
                    {
                        yield return new WaitForSecondsRealtime(0.02f);
                    }
                    else
                    {
                        yield return new WaitForSecondsRealtime(animBuilder.FlyingTweenInterval);
                    }
                }
            }
        }

        public class AnimBuilder
        {
            public FlyAnim Anim;
            public int InstanceCount
            {
                get; private set;
            }
            public float MinAnimRadius
            {
                get; private set;
            }
            public float MaxAnimRadius
            {
                get; private set;
            }
            public float FlyingPosTime
            {
                get; private set;
            }
            public float InitializeInstanceInterval
            {
                get; private set;
            }
            public float WaitForFlyingTargetInterval
            {
                get; private set;
            }
            public float FlyingTargetTime
            {
                get; private set;
            }
            public float FlyingTweenInterval
            {
                get; private set;
            }
            public float StartAngle
            {
                get; private set;
            }
            public float EndAngle
            {
                get; private set;
            }

            public Ease Ease
            {
                get; private set;
            }

            public Action<FlyObject> OnStartFly;

            public Transform spawnTransform;
            public Transform FlyTargetTransform
            {
                get; private set;
            }
            public Action OnFlyingEnd;
            public void SetInstanceCount(int instanceCount)
            {
                InstanceCount = instanceCount;
            }
            public void SetMinAnimRadius(float animRadius)
            {
                MinAnimRadius = animRadius;
            }
            public void SetMaxAnimRadius(float animRadius)
            {
                MaxAnimRadius = animRadius;
            }
            public void SetFlyingPosTime(float flyPosTime)
            {
                FlyingPosTime = flyPosTime;
            }
            public void SetInitializeInstanceInterval(float initializeInstanceInterval)
            {
                InitializeInstanceInterval = initializeInstanceInterval;
            }
            public void SetWaitForFlyingTargetInterval(float waitForFlyTargetTime)
            {
                WaitForFlyingTargetInterval = waitForFlyTargetTime;
            }
            public void SetFlyingTargetTime(float flyTargetTime)
            {
                FlyingTargetTime = flyTargetTime;
            }
            public void SetFlyingTweenInterval(float flyingTweenInterval)
            {
                FlyingTweenInterval = flyingTweenInterval;
            }
            public void SetStartAngle(float startAngle)
            {
                StartAngle = startAngle;
            }
            public void SetEndAngle(float endAngle)
            {
                EndAngle = endAngle;
            }
            public void SetSpawnTransform(Transform spawnTransform)
            {
                this.spawnTransform = spawnTransform;
            }
            public void SetFlyTargetTransform(Transform flyTargetTransform)
            {
                FlyTargetTransform = flyTargetTransform;
            }
            public void SetEase(Ease ease)
            {
                Ease = ease;
            }
            public void SetOnStartFly(Action<FlyObject> onStartFly)
            {
                OnStartFly = onStartFly;
            }
            public Coroutine Build()
            {
                return Anim.StartCoroutine(Anim.PlayMoneyAnim(this));
            }
        }
    }
}
