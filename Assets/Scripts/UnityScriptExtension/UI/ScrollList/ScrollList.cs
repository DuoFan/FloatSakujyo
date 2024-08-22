using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace GameExtension
{
    public class ScrollList : MonoBehaviour
    {
        [SerializeField] protected ScrollRect scrollRect;
        public ScrollRect ScrollRect => scrollRect;

        [SerializeField] protected ScrollListItem itemPrototype;
        Func<ScrollListItem> itemProvider;
        Action<ScrollListItem> itemRecycler;

        [SerializeField] LayoutDirection layoutDirection;
        ScrollStrategy scrollStrategy;

        /// <summary>
        /// 元素与内容区域水平方向和垂直方向的偏移
        /// </summary>
        public Vector2 padding;

        /// <summary>
        /// 元素大小
        /// </summary>
        public Vector2 itemSize;

        [Header("设置该项时itemSize.x不会生效")]
        public bool isDontSetWidth;

        [Header("设置该项时itemSize.y不会生效")]
        public bool isDontSetHeight;

        [Header("设置该项时不会改变Item的锚点")]
        public bool isDontSetItemAnchor;

        /// <summary>
        /// 元素的水平间隔和垂直间隔
        /// </summary>
        public Vector2 itemSpacing;

        /// <summary>
        /// 可见的列数和行数
        /// </summary>
        public Vector2Int visualSize;

        protected int curVisualMaxRow;

        [SerializeField] float visibleThreshold = 128f;

        Vector2 lastPos;

        private void Awake()
        {
            scrollRect.onValueChanged.AddListener(UpdateItemList);
        }

        public void AdaptVisualWidthByViewPort()
        {
            visualSize.x = Mathf.FloorToInt(scrollRect.viewport.rect.width / (itemSize.x + itemSpacing.x));
            if(visualSize.x <= 0)
            {
                visualSize.x = 1;
            }
        }


        public void SetDatas<T>(T[] datas, Comparison<T> comparison) where T : IScrollListData
        {
            scrollRect.enabled = true;
            if (scrollStrategy == null)
            {
                scrollStrategy = GetScrollStrategy(layoutDirection);
            }

            scrollStrategy.SetDatas(datas, comparison);
        }

        public void SetFuncs(Func<ScrollListItem> itemProvider, Action<ScrollListItem> itemRecycler)
        {
            this.itemProvider = itemProvider;
            this.itemRecycler = itemRecycler;
        }

        public void UpdateItemList()
        {
            UpdateItemList(scrollRect.normalizedPosition);
        }

        void UpdateItemList(Vector2 pos)
        {
            if (pos.y > 1 || pos.y < 0)
            {
                return;
            }

            var delta = pos - lastPos;
            lastPos = pos;
            scrollStrategy.UpdateItemList(delta);
        }

        public void Clear()
        {
            scrollRect.velocity = Vector2.zero;
            scrollRect.enabled = false;
            scrollStrategy?.ClearItems();
        }

        public Vector2 GetDataPos(IScrollListData data)
        {
            return scrollStrategy.GetDataPos(data);
        }

        public RectTransform GetItemContent()
        {
            return scrollStrategy.GetItemContent();
        }
        public List<ScrollListItem> GetItems()
        {
            return new List<ScrollListItem>(scrollStrategy.Items);
        }

        ScrollStrategy GetScrollStrategy(LayoutDirection layoutDirection)
        {
            switch (layoutDirection)
            {
                case LayoutDirection.Up:
                case LayoutDirection.Down:
                    return new VerticalScrollStrategy(this);
            }

            return null;
        }

        enum LayoutDirection
        {
            Up,
            Down,
            Left,
            Right,
        }

        abstract class ScrollStrategy
        {
            public List<ScrollListItem> Items { get; protected set; }
            protected ScrollList scrollList;
            protected Pool<ScrollListItem> itemPool;
            protected List<IScrollListData> allData;
            protected Dictionary<IScrollListData, Vector2> dataPosMap;
            protected RectTransform itemContent;

            public ScrollStrategy(ScrollList _scrollList)
            {
                scrollList = _scrollList;
                Items = new List<ScrollListItem>();
                allData = new List<IScrollListData>();
                dataPosMap = new Dictionary<IScrollListData, Vector2>();
                itemContent = new GameObject("ItemContent", typeof(RectTransform)).GetComponent<RectTransform>();
                itemContent.transform.SetParent(scrollList.scrollRect.content);
                itemContent.localScale = Vector3.one;
                itemContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scrollList.scrollRect.content.rect.width);
                itemContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scrollList.scrollRect.content.rect.height);
                if (scrollList.itemProvider == null)
                {
                    itemPool = new Pool<ScrollListItem>(() =>
                    {
                        var go = GameObject.Instantiate(scrollList.itemPrototype, itemContent);
                        var rectRransform = go.GetComponent<RectTransform>();
                        var item = go.GetComponent<ScrollListItem>();
                        item.SetSize(scrollList.itemSize, scrollList.isDontSetWidth, scrollList.isDontSetHeight);
                        return item;
                    }, 30);
                }
            }

            public virtual void ClearItems()
            {
                allData.Clear();
                dataPosMap.Clear();
                for (int i = 0; i < Items.Count; i++)
                {
                    ReturnItem(Items[i]);
                }

                Items.Clear();
            }

            public abstract void SetDatas<T>(T[] datas, Comparison<T> comparison) where T : IScrollListData;
            public abstract void UpdateItemList(Vector2 delta);

            public Vector2 GetDataPos(IScrollListData data)
            {
                return dataPosMap[data];
            }

            protected ScrollListItem GetItem(IScrollListData data)
            {
                ScrollListItem item = null;
                if (scrollList.itemProvider != null)
                {
                    item = scrollList.itemProvider();
                }
                else
                {
                    item = itemPool.Get();
                }
                item.gameObject.CheckActiveSelf(true);
                item.transform.SetParent(itemContent);
                item.SetData(data);
                item.transform.localScale = Vector3.one;
                return item;
            }

            protected void ReturnItem(ScrollListItem item)
            {
                item.OnReturn();
                item.data = null;
                item.gameObject.CheckActiveSelf(false);
                item.transform.SetParent(scrollList.transform);
                if (scrollList.itemRecycler != null)
                {
                    scrollList.itemRecycler(item);
                }
                else
                {
                    itemPool.Return(item);
                }
            }

            protected abstract ScrollListItem InitItemForData(IScrollListData data);

            public RectTransform GetItemContent() => itemContent;


        }

        class VerticalScrollStrategy : ScrollStrategy
        {
            float height;
            bool isLayoutUp;

            public VerticalScrollStrategy(ScrollList _scrollList) : base(_scrollList)
            {
                isLayoutUp = scrollList.layoutDirection == LayoutDirection.Up;
                itemContent.SetAnchor(ScriptExtension.AnchorPresets.HorStretchMiddle);
                /*if (isLayoutUp)
                {
                    //scrollList.scrollRect.content.SetAnchor(ScriptExtension.AnchorPresets.BottomCenter);
                    //scrollList.scrollRect.content.SetPivot(ScriptExtension.PivotPresets.BottomCenter);
                    itemContent.SetAnchor(ScriptExtension.AnchorPresets.HorStretchBottom);
                    itemContent.SetPivot(ScriptExtension.PivotPresets.BottomCenter);
                }
                else
                {
                    //scrollList.scrollRect.content.SetAnchor(ScriptExtension.AnchorPresets.TopLeft);
                    //scrollList.scrollRect.content.SetPivot(ScriptExtension.PivotPresets.TopLeft);
                    itemContent.SetAnchor(ScriptExtension.AnchorPresets.HorStretchTop);
                    itemContent.SetPivot(ScriptExtension.PivotPresets.TopCenter);
                }*/

                itemContent.sizeDelta = new Vector2(0, scrollList.scrollRect.content.rect.height);

                itemContent.anchoredPosition3D = Vector3.zero;
            }

            public override void SetDatas<T>(T[] datas, Comparison<T> comparison)
            {
                var visualSize = scrollList.visualSize;
                int maxCount = visualSize.x * visualSize.y;
                if (comparison != null)
                {
                    Array.Sort(datas, comparison);
                }

                Queue<T> waitToFitDatas = new Queue<T>(datas);
                var itemHeight = scrollList.itemSize.y;
                var itemWidth = scrollList.itemSize.x;
                var xStart = scrollList.padding.x;
                var yStart = scrollList.padding.y;
                var xSpacing = scrollList.itemSpacing.x;
                var ySpacing = scrollList.itemSpacing.y;
                height = isLayoutUp ? yStart : -yStart;
                height = FixDataHeight(height);

                float FixDataHeight(float startHeight)
                {
                    int index = 0;
                    float _XStart = xStart;
                    int curRow = 0;
                    float stepHeight = itemHeight + ySpacing;
                    float stepWidth = itemWidth + xSpacing;
                    while (waitToFitDatas.Count > 0)
                    {
                        var data = waitToFitDatas.Dequeue();
                        int col = index % visualSize.x;
                        int row = index / visualSize.x;
                        if (curRow < row)
                        {
                            curRow = row;
                            _XStart = xStart;
                            startHeight += isLayoutUp ? stepHeight : -stepHeight;
                        }
                        else if (col > 0)
                        {
                            _XStart += stepWidth;
                        }

                        allData.Add(data);
                        dataPosMap.Add(data, new Vector2(_XStart, startHeight));
                        index++;
                    }

                    return startHeight;
                }

                ScrollListItem item;
                for (int i = Items.Count, j = 0; i < maxCount && i < datas.Length; i++, j++)
                {
                    var data = datas[i];
                    item = InitItemForData(data);
                    //添加在显示面板时的初始化方法
                    item.Init();
                    Items.Add(item);
                }

                height = Mathf.Abs(height) + itemHeight;
                itemContent.sizeDelta = new Vector2(0, height);
                scrollList.scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                scrollList.scrollRect.verticalNormalizedPosition = isLayoutUp ? 0 : 1;
                scrollList.lastPos = isLayoutUp ? new Vector2(0, 0) : new Vector2(0, 1);
            }

            public override void UpdateItemList(Vector2 delta)
            {
                if (delta.y == 0)
                {
                    return;
                }

                var scrollRect = scrollList.scrollRect;
                var viewPortHeight = scrollRect.viewport.rect.height;
                var contentPosY = Mathf.Abs(scrollRect.content.anchoredPosition.y);
                var top = isLayoutUp ? contentPosY + viewPortHeight : -contentPosY;
                var bottom = top - viewPortHeight;
                top += scrollList.visibleThreshold;
                bottom -= scrollList.visibleThreshold;
                var itemHeight = scrollList.itemSize.y;
                bool isScrollUp = delta.y > 0;
                Queue<ScrollListItem> waitToReturnItems = new Queue<ScrollListItem>();
                List<IScrollListData> waitToShowDatas = new List<IScrollListData>();

                //从该数据开始往后一位,寻找所有可见的数据
                IScrollListData startVisibleData;

                #region ReturnInvisibleItem

                bool CheckDataIsInvisible(IScrollListData data)
                {
                    var pivotHeight = dataPosMap[data].y;
                    var anotherPivotHeight = pivotHeight;
                    //向上滑动,寻找顶部高度
                    if (isScrollUp)
                    {
                        //此时元素的轴心在底部,要加上元素自身高度
                        if (isLayoutUp)
                        {
                            anotherPivotHeight += itemHeight;
                        }
                    }
                    //向下滑动,寻找底部高度
                    else
                    {
                        //此时元素的轴心在顶部,要减去元素自身高度
                        if (!isLayoutUp)
                        {
                            anotherPivotHeight -= itemHeight;
                        }
                    }

                    bool isPivotOutside = pivotHeight > top || pivotHeight < bottom;
                    bool isAnotherPivotOutside = anotherPivotHeight > top || anotherPivotHeight < bottom;
                    return isPivotOutside && isAnotherPivotOutside;
                }

                int removeStart;
                int startIndex;
                int endIndex;
                int searchDir;
                //向上滑动,搜索看不见的元素,直到找到顶部高度在Top和Bottom之间的元素
                if (isScrollUp)
                {
                    //上方布局,从头元素搜索
                    if (isLayoutUp)
                    {
                        //                                         ---------- Top
                        //      M N O P  (Invisible) =>             M N O P  (Visible)                      
                        //      ---------- Top                      I J K L   
                        //      I J K L(startVisibleData) (New Head)E F G H
                        //      E F G H                            ---------- Bottom
                        //(Head)A B C D  (Visible)   =>             A B C D  (Invisible)
                        //      ---------- Bottom     

                        //搜索路径
                        //  A->B->C->D->E(Visible 中断)  
                        startIndex = 0;
                        searchDir = 1;
                    }
                    //下方布局,从尾元素搜索
                    else
                    {
                        //                                         ---------- Top
                        //      M N O P  (Invisible) =>             M N O P  (Visible)                      
                        //      ---------- Top                      I J K L   
                        //(startVisibleData)I J K L                             E F G H(New Tail)
                        //      E F G H                            ---------- Bottom
                        //      A B C D(Tail)(Visible)   =>         A B C D  (Invisible)
                        //      ---------- Bottom     

                        //搜索路径           
                        //  D->C->B->A->H(Visible 中断)  
                        startIndex = Items.Count - 1;
                        searchDir = -1;
                    }
                }
                //向下滑动,搜索看不见的元素,直到找到底部高度在Top和Bottom之间的元素
                else
                {
                    //上方布局,从尾元素搜索
                    if (isLayoutUp)
                    {
                        //      ---------- Top                 
                        //      M N O P(Tail)(Visible)   =>     M N O P   (Invisible)
                        //      I J K L                        ---------- Top
                        //(startVisibleData)E F G H                         I J K L(New Tail)
                        //      ---------- Bottom               E F G H
                        //      A B C D     (Invisible)  =>     A B C D   (Visible)
                        //                                     ---------- Bottom
                        //搜索路径
                        //  P->O->M->M->L(Visible 中断)  
                        startIndex = startIndex = Items.Count - 1;
                        searchDir = -1;
                    }
                    //下方布局,从头元素搜索
                    else
                    {
                        //      ---------- Top                 
                        //(Head)M N O P      (Visible)   =>          M N O P   (Invisible)
                        //      I J K L                             ---------- Top
                        //      E F G H(startVisibleData)  (New Head)I J K L
                        //      ---------- Bottom                    E F G H
                        //      A B C D     (Invisible)  =>          A B C D   (Visible)
                        //                                          ---------- Bottom
                        //搜索路径
                        //  M->N->O->P->I(Visible 中断)  
                        startIndex = 0;
                        searchDir = 1;
                    }
                }

                endIndex = Items.Count - 1 - startIndex + searchDir;
                startVisibleData = Items[endIndex - searchDir].data;
                for (int i = startIndex; i != endIndex; i += searchDir)
                {
                    if (CheckDataIsInvisible(Items[i].data))
                    {
                        waitToReturnItems.Enqueue(Items[i]);
                    }
                    else break;
                }

                //从头往尾搜索
                if (startIndex < endIndex)
                {
                    removeStart = startIndex;
                }
                //从尾往头搜索
                else
                {
                    removeStart = startIndex - waitToReturnItems.Count + 1;
                }

                //所有元素都被回收,说明列表被大幅度滑动
                bool isAllReturn = waitToReturnItems.Count == Items.Count;
                if (isAllReturn)
                {
                    Items.Clear();
                }
                else if (waitToReturnItems.Count > 0)
                {
                    Items.RemoveRange(removeStart, waitToReturnItems.Count);
                }

                while (waitToReturnItems.Count > 0)
                {
                    var item = waitToReturnItems.Dequeue();
                    ReturnItem(item);
                }

                #endregion

                #region ShowVisibleItem

                bool CheckDataIsVisible(IScrollListData data)
                {
                    var height = dataPosMap[data].y;
                    //向上滑动,寻找底部高度
                    if (isScrollUp)
                    {
                        //此时元素的轴心在顶部,要减去元素自身高度
                        if (!isLayoutUp)
                        {
                            height -= itemHeight;
                        }
                    }
                    //向下滑动,寻找顶部高度
                    else
                    {
                        //此时元素的轴心在底部,要加上元素自身高度
                        if (isLayoutUp)
                        {
                            height += itemHeight;
                        }
                    }

                    return height <= top && height >= bottom;
                }

                //向上滑动,搜索看得见的元素,直到找到底部高度不在Top和Bottom之间的元素
                //向下滑动,搜索看得见的元素,直到找到顶部高度不在Top和Bottom之间的元素
                if (isScrollUp == isLayoutUp)
                {
                    endIndex = allData.Count;
                    searchDir = 1;
                }
                else
                {
                    endIndex = -1;
                    searchDir = -1;
                }

                startIndex = allData.IndexOf(startVisibleData) + searchDir;
                for (int i = startIndex; i != endIndex; i += searchDir)
                {
                    if (CheckDataIsVisible(allData[i]))
                    {
                        waitToShowDatas.Add(allData[i]);
                    }
                    else if (!isAllReturn || waitToShowDatas.Count > 0)
                    {
                        break;
                    }
                }

                if (isScrollUp == isLayoutUp)
                {
                    startIndex = 0;
                    endIndex = waitToShowDatas.Count;
                    searchDir = 1;
                }
                else
                {
                    startIndex = waitToShowDatas.Count - 1;
                    endIndex = -1;
                    searchDir = -1;
                }

                if (waitToShowDatas.Count > 0)
                {
                    ScrollListItem[] showItems = new ScrollListItem[waitToShowDatas.Count];
                    int k = 0;
                    for (int i = startIndex; i != endIndex; i += searchDir, k++)
                    {
                        var data = waitToShowDatas[i];
                        var item = InitItemForData(data);
                        showItems[k] = item;
                    }

                    if (isScrollUp == isLayoutUp)
                    {
                        Items.InsertRange(Items.Count, showItems);
                    }
                    else
                    {
                        Items.InsertRange(0, showItems);
                    }
                }

                #endregion
            }

            protected override ScrollListItem InitItemForData(IScrollListData data)
            {
                var item = GetItem(data);
                var rectRransform = item.transform as RectTransform;
                if (!scrollList.isDontSetItemAnchor)
                {
                    if (isLayoutUp)
                    {
                        rectRransform.SetAnchor(ScriptExtension.AnchorPresets.BottomLeft);
                    }
                    else
                    {
                        rectRransform.SetAnchor(ScriptExtension.AnchorPresets.TopLeft);
                    }
                }

                if (isLayoutUp)
                {
                    rectRransform.SetPivot(ScriptExtension.PivotPresets.BottomCenter);
                }
                else
                {
                    rectRransform.SetPivot(ScriptExtension.PivotPresets.TopLeft);
                }

                item.SetSize(scrollList.itemSize, scrollList.isDontSetWidth, scrollList.isDontSetHeight);
                item.SetPos(dataPosMap[data]);
                return item;
            }

            public override void ClearItems()
            {
                base.ClearItems();
                scrollList.scrollRect.verticalNormalizedPosition = isLayoutUp ? 0 : 1;
            }
        }
    }
}