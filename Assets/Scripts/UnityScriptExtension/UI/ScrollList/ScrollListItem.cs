using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameExtension
{
    public class ScrollListItem : MonoBehaviour
    {
        public IScrollListData data;

        RectTransform rectTransform;

        /// <summary>
        /// 物体初始化操作
        /// </summary>
        public virtual void Init()
        {

        }

        public virtual void SetSize(Vector2 size, bool isDontSetWidth, bool isDontSetHeight)
        {
            if (rectTransform == null)
            {
                rectTransform = transform as RectTransform;
            }
            if (!isDontSetWidth)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            }
            if(!isDontSetHeight)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            }
        }
        public virtual void SetPos(Vector2 pos)
        {
            if (rectTransform == null)
            {
                rectTransform = transform as RectTransform;
            }
            rectTransform.anchoredPosition3D = pos;
        }
        public virtual void SetData(IScrollListData _data)
        {
            data = _data;
        }
        public virtual void OnReturn()
        {

        }
    }

}