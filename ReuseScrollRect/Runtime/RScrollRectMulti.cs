using UnityEngine;

namespace ReuseScrollRect.Runtime
{
    public abstract class RScrollRectMulti : RScrollRectBase
    {
        protected override void ItemRefreshEvent(Transform transform, int index)
        {
            this.itemFactory?.ItemRefreshEvent(transform, index);
        }
        
        // Multi Data Source cannot support TempPool
        protected override RectTransform GetFromTempPool(int itemIdx)
        {
            RectTransform nextItem = (RectTransform) itemFactory.InstantiateOneItem(itemIdx).transform;
            nextItem.transform.SetParent(m_Content, false);
            nextItem.gameObject.SetActive(true);

            ItemRefreshEvent(nextItem, itemIdx);
            return nextItem;
        }

        protected override void ReturnToTempPool(bool fromStart, int count)
        {
            Debug.Assert(m_Content.childCount >= count);
            if (fromStart)
            {
                for (int i = count - 1; i >= 0; i--)
                {
                    itemFactory.RecycleOneItem(m_Content.GetChild(i));
                }
            }
            else
            {
                int t = m_Content.childCount - count;
                for (int i = m_Content.childCount - 1; i >= t; i--)
                {
                    itemFactory.RecycleOneItem(m_Content.GetChild(i));
                }
            }
        }

        protected override void ClearTempPool()
        {
        }
    }
}