using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameExtension.GameObjectTrace;

namespace GameExtension
{
    public class GameObjectTrace : MonoBehaviour
    {
        public static void TraceGameObject(Transform tracer, Transform target, Vector3 offset,
            ITraceStrategy _traceStrategy, TraceMode _traceMode = TraceMode.X | TraceMode.Y | TraceMode.Z)
        {
            if (tracer != null && target != null && _traceStrategy != null)
            {
                var trace = tracer.GetComponent<GameObjectTrace>();
                if (trace == null)
                {
                    trace = tracer.gameObject.AddComponent<GameObjectTrace>();
                }
                trace.TraceTarget(target, offset, _traceStrategy, _traceMode);
                trace.Enable();
            }
        }


        public Transform target;
        public Vector3 targetOffset;
        public bool IsTraceX => traceMode.HasFlag(TraceMode.X);
        public bool IsTraceY => traceMode.HasFlag(TraceMode.Y);
        public bool IsTraceZ => traceMode.HasFlag(TraceMode.Z);
        public bool isEnable;
        public ITraceStrategy traceStrategy;
        public TraceMode traceMode;

        GameObjectTraceContext traceContext;

        public void TraceTarget(Transform _target, Vector3 _offset, ITraceStrategy _traceStrategy, TraceMode _traceMode = TraceMode.X | TraceMode.Y | TraceMode.Z)
        {
            target = _target;
            targetOffset = _offset;
            traceStrategy = _traceStrategy;
            traceMode = _traceMode;

            traceStrategy.OnStartTraceTarget(GetTraceContext());

            Enable();
            Update();
        }
        public void Enable()
        {
            isEnable = true;
        }
        public void Disable()
        {
            isEnable = false;
        }

        private void Update()
        {
            if (target != null && isEnable && traceStrategy != null)
            {
                traceStrategy.TraceTarget(GetTraceContext());
            }
        }

        GameObjectTraceContext GetTraceContext()
        {
            traceContext.transform = transform;
            traceContext.target = target;
            traceContext.targetOffset = targetOffset;
            traceContext.traceMode = traceMode;
            return traceContext;
        }


        public struct GameObjectTraceContext
        {
            public Transform transform;
            public Transform target;
            public Vector3 targetOffset;
            public TraceMode traceMode;
        }

        [Flags]
        public enum TraceMode
        {
            X = 1, Y = 2, Z = 4
        }

        public interface ITraceStrategy : ICloneable
        {
            public void OnStartTraceTarget(GameObjectTraceContext traceContext);
            public void TraceTarget(GameObjectTraceContext traceContext);
        }

        public class DirectTraceStrategy : ITraceStrategy
        {

            public static DirectTraceStrategy SHARED = new DirectTraceStrategy();

            public void OnStartTraceTarget(GameObjectTraceContext traceContext)
            {

            }

            public void TraceTarget(GameObjectTraceContext traceContext)
            {
                var targetPos = traceContext.target.position + traceContext.targetOffset;

                if (!traceContext.traceMode.HasFlag(TraceMode.X)) targetPos.x = traceContext.transform.position.x;
                if (!traceContext.traceMode.HasFlag(TraceMode.Y)) targetPos.y = traceContext.transform.position.y;
                if (!traceContext.traceMode.HasFlag(TraceMode.Z)) targetPos.z = traceContext.transform.position.z;

                traceContext.transform.position = targetPos;
            }
            public object Clone()
            {
                return new DirectTraceStrategy();
            }
        }

        public class SmoothTraceStrategy : ITraceStrategy
        {
            private Vector3 currentVelocity;

            //平滑时间,控制平滑过渡的速度,较小的值会更快地接近目标,而较大的值会更慢。
            public float smoothTime = 0.5f;
            //弹簧最大速度
            public float maxSpeed = 10f;
            //阈值内不会进行追踪
            public float threshold;

            public void OnStartTraceTarget(GameObjectTraceContext traceContext)
            {

            }

            public void TraceTarget(GameObjectTraceContext traceContext)
            {
                var targetPos = traceContext.target.position - traceContext.targetOffset;

                if (!traceContext.traceMode.HasFlag(TraceMode.X)) targetPos.x = traceContext.transform.position.x;
                if (!traceContext.traceMode.HasFlag(TraceMode.Y)) targetPos.y = traceContext.transform.position.y;
                if (!traceContext.traceMode.HasFlag(TraceMode.Z)) targetPos.z = traceContext.transform.position.z;

                if (Vector3.Distance(traceContext.transform.position, targetPos) < threshold)
                {
                    return;
                }

                targetPos = Vector3.SmoothDamp(
                    traceContext.transform.position,
                    targetPos,
                    ref currentVelocity,
                    smoothTime,
                    maxSpeed,
                    Time.deltaTime
                );

                traceContext.transform.position = targetPos;
            }

            public object Clone()
            {
                return new SmoothTraceStrategy()
                {
                    smoothTime = smoothTime,
                    maxSpeed = maxSpeed,
                    threshold = threshold
                };
            }
        }
    }
}

