using DG.Tweening;
using GameExtension;
using System;
using System.Collections;
using UnityEngine;

namespace GameExtension
{
    public class CameraController : SingletonMonoBase<CameraController>
    {
        public Camera Camera => mCamera;

        [SerializeField] Camera mCamera;

        public Vector3 GetPosition()
        {
            return Camera.transform.position;
        }

        public void SetPosition(Vector2 pos)
        {
            Camera.transform.position = TransformCameraPos(pos);
        }

        public Tween PositionTo(Vector3 targetPos, float time, float waitTime = 0, bool isTransitionBack = false, float backTime = -1)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(Camera.transform.DOMove(TransformCameraPos(targetPos), time));
            if (waitTime > 0)
            {
                sequence.AppendInterval(waitTime);
            }
            if (isTransitionBack)
            {
                backTime = backTime < 0 ? time : backTime;
                var oldPos = Camera.transform.position;
                var tween = Camera.transform.DOMove(TransformCameraPos(oldPos), backTime);
                sequence.Append(tween);
            }
            return sequence;
        }

        public void SetOrthoSize(float targetOrtho)
        {
            Camera.orthographicSize = targetOrtho;
        }

        public Tween OrthoSizeTo(float targetOrtho, float time, float waitTime = 0, bool isTransitionBack = false, float backTime = -1)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(Camera.DOOrthoSize(targetOrtho, time));
            if (waitTime > 0)
            {
                sequence.AppendInterval(waitTime);
            }
            if (isTransitionBack)
            {
                backTime = backTime < 0 ? time : backTime;
                float oldSize = Camera.orthographicSize;
                var tween = Camera.DOOrthoSize(oldSize, backTime);
                sequence.Append(tween);
            }
            return sequence;
        }

        public Tween OrthoSizeToDefault(float time)
        {
            return OrthoSizeTo(5, time, 0, false);
        }

        public Tween FocusOrthoSize(int targetOrthoSize)
        {
            return OrthoSizeTo(targetOrthoSize, 0.5f, 0, false);
        }

        public float GetFov()
        {
            return Camera.fieldOfView;
        }
        public void SetFov(float targetFov)
        {
            Camera.fieldOfView = targetFov;
        }
        public Tween FovTo(float targetFov, float time, float waitTime, bool isTransitionBack, float backTime = -1)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(mCamera.DOFieldOfView(targetFov, time));
            if (waitTime > 0)
            {
                sequence.AppendInterval(waitTime);
            }
            if (isTransitionBack)
            {
                backTime = backTime < 0 ? time : backTime;
                float oldFov = Camera.fieldOfView;
                var tween = Camera.DOFieldOfView(oldFov, backTime);
                sequence.Append(tween);
            }
            return sequence;
        }
        public Tween FovToDefault(float time)
        {
            return FovTo(60, time, 0, false);
        }
        public Tween FocusFov(int targetFov)
        {
            return FovTo(targetFov, 0.5f, 0, false);
        }

        Vector3 TransformCameraPos(Vector3 pos)
        {
            return new Vector3(pos.x, pos.y, mCamera.transform.position.z);
        }

        public void SetRotation(Quaternion rotation)
        {
            Camera.transform.rotation = rotation;
        }
    }
}