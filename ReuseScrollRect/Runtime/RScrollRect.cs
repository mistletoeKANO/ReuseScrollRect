using System;
using UnityEngine;

namespace ReuseScrollRect.Runtime
{
    public abstract class RScrollRect : RScrollRectBase
    {
        /// <summary>
        /// init scrollRect.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="maxCount"></param>
        public void InitReuseScrollRect(IItemFactory factory, int maxCount)
        {
            this.itemFactory = factory ?? throw new ArgumentNullException(nameof(factory), "Factory is null.");
            this.totalCount = maxCount;
            this.RefillCells();
        }

        /// <summary>
        /// 重新显示刷新布局
        /// </summary>
        /// <param name="maxCount"></param>
        public void ReShowScrollRect(int maxCount)
        {
            this.totalCount = maxCount;
            this.RefillCells();
        }

        /// <summary>
        /// refresh scroll rect when show item list changed.
        /// </summary>
        /// <param name="maxCount"></param>
        public void RefreshScrollRect(int maxCount)
        {
            this.totalCount = maxCount;
            this.RefreshCells();
        }

        protected override void ItemRefreshEvent(Transform trans, int index)
        {
            this.itemFactory?.ItemRefreshEvent(trans, index);
        }
        
        protected override RectTransform GetFromTempPool(int itemIdx)
        {
            RectTransform nextItem = null;
            if (deletedItemTypeStart > 0)
            {
                deletedItemTypeStart--;
                nextItem = (RectTransform) m_Content.GetChild(0);
                nextItem.SetSiblingIndex(itemIdx - startItemIndex + deletedItemTypeStart);
            }
            else if (deletedItemTypeEnd > 0)
            {
                deletedItemTypeEnd--;
                nextItem = (RectTransform) m_Content.GetChild(m_Content.childCount - 1);
                nextItem.SetSiblingIndex(itemIdx - startItemIndex + deletedItemTypeStart);
            }
            else
            {
                nextItem = (RectTransform) itemFactory.InstantiateOneItem(itemIdx).transform;
                nextItem.transform.SetParent(m_Content, false);
                nextItem.gameObject.SetActive(true);
            }
            ItemRefreshEvent(nextItem, itemIdx);
            return nextItem;
        }

        protected override void ReturnToTempPool(bool fromStart, int count)
        {
            if (fromStart)
                deletedItemTypeStart += count;
            else
                deletedItemTypeEnd += count;
        }

        protected override void ClearTempPool()
        {
            Debug.Assert(m_Content.childCount >= deletedItemTypeStart + deletedItemTypeEnd);
            if (deletedItemTypeStart > 0)
            {
                for (int i = deletedItemTypeStart - 1; i >= 0; i--)
                {
                    itemFactory.RecycleOneItem(m_Content.GetChild(i));
                }
                deletedItemTypeStart = 0;
            }
            if (deletedItemTypeEnd > 0)
            {
                int t = m_Content.childCount - deletedItemTypeEnd;
                for (int i = m_Content.childCount - 1; i >= t; i--)
                {
                    itemFactory.RecycleOneItem(m_Content.GetChild(i));
                }
                deletedItemTypeEnd = 0;
            }
        }
    }
}