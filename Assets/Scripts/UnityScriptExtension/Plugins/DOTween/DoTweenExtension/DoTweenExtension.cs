using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static GameExtension.ScriptExtension;

namespace GameExtension
{
    public static class DoTweenExtension
    {
        public enum MoveDir
        {
            Up = 1, Down = 2, Left = 4, Right = 8
        }

        public static RectTransform HideInCanvas(this RectTransform transform, MoveDir dir, float dur)
        {
            var anchord = transform.GetAnchor().Value;
            if (anchord > AnchorPresets.TopRight)
                throw new System.Exception("暂不支持");
            Vector2 dis = Vector2.zero;
            int dif = 0;
            switch (dir)
            {
                case MoveDir.Up:
                    if ((dif = 2 - (int)anchord % 3) == 0) dis = transform.anchoredPosition.y.Abs() * Vector2.up;
                    else dis = transform.GetOffset((AnchorPresets)((int)anchord + dif));
                    dis += transform.rect.height / 2 * Vector2.up;
                    break;
                case MoveDir.Down:
                    if ((dif = (int)anchord % 3) == 0) dis = transform.anchoredPosition.y.Abs() * Vector2.down;
                    else dis = transform.GetOffset((AnchorPresets)((int)anchord - dif));
                    dis += transform.rect.height / 2 * Vector2.down;
                    break;
                case MoveDir.Left:
                    if (anchord > AnchorPresets.TopLeft)
                    {
                        while (anchord > AnchorPresets.TopLeft) anchord = (AnchorPresets)(int)anchord - 3;
                        dis = transform.GetOffset(anchord);
                    }
                    else dis = transform.anchoredPosition.x.Abs() * Vector2.left;
                    dis += transform.rect.width / 2 * Vector2.left;
                    break;
                case MoveDir.Right:
                    if (anchord < AnchorPresets.BottomRight)
                    {
                        while (anchord < AnchorPresets.BottomRight) anchord = (AnchorPresets)(int)anchord + 3;
                        dis = transform.GetOffset(anchord);
                    }
                    else dis = transform.anchoredPosition.x.Abs() * Vector2.right;
                    dis += transform.rect.width / 2 * Vector2.right;
                    break;
            }
            transform.DOAnchorPos3D(dis, dur).SetRelative();
            return transform;
        }
        public static RectTransform ShowInCanvas(this RectTransform transform, float dur)
        {
            var anchord = transform.GetAnchor().Value;
            if (anchord > AnchorPresets.TopRight)
                throw new System.Exception("暂不支持");
            float x = transform.anchoredPosition.x;
            float y = transform.anchoredPosition.y;
            var parentRect = (transform.parent as RectTransform).rect;
            float xBorder1 = (1 - transform.anchorMin.x) * parentRect.width - transform.rect.width / 2;
            float xBorder2 = -(transform.anchorMin.x * parentRect.width - transform.rect.width / 2);
            float yBorder1 = (1 - transform.anchorMin.y) * parentRect.height - transform.rect.height / 2;
            float yBorder2 = -(transform.anchorMin.y * parentRect.height - transform.rect.height / 2);
            if (x > xBorder2 && x < xBorder1 && y > yBorder1 && y < yBorder2)
                throw new System.Exception("已在画布中显示");
            Vector2 dis = Vector2.zero;
            if (!(x > xBorder2 && x < xBorder1))
                dis += ((xBorder1 - x).Abs() < (xBorder2 - x).Abs() ? xBorder1 - x : xBorder2 - x) * Vector2.right;
            if (!(y > yBorder2 && y < yBorder1))
                dis += ((yBorder1 - y).Abs() < (yBorder2 - y).Abs() ? yBorder1 - y : yBorder2 - y) * Vector2.up;
            transform.DOAnchorPos3D(dis, dur).SetRelative();
            return transform;
        }
        static Vector2 GetOffset(this RectTransform transform, AnchorPresets targetPre)
        {
            Vector2 result = Vector2.zero;
            var parentRect = (transform.parent as RectTransform).rect;
            var ori = transform.anchorMin;
            var target = anchordDic[targetPre][0];
            if (ori.y != target.y)
            {
                var y = target.y - ori.y;
                y *= parentRect.height;
                result += y * Vector2.up;
                result -= transform.anchoredPosition.y * Vector2.up;
            }
            if (ori.x != target.x)
            {
                var x = target.x - ori.x;
                x *= parentRect.width;
                result += x * Vector2.right;
                result -= transform.anchoredPosition.x * Vector2.right;
            }
            return result;
        }
    }
}


