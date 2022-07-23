using UnityEngine;
using UnityEngine.UI;

namespace ReuseScrollRect.Runtime
{
    [AddComponentMenu("UI/Reuse Horizontal Scroll Rect(MultiPrefab)", 52)]
    [DisallowMultipleComponent]
    public class RHorizontalScrollRectMulti : RScrollRectMulti
    {
        protected override float GetSize(RectTransform item, bool includeSpacing)
        {
            float size = includeSpacing ? contentSpacing : 0;
            if (m_GridLayout != null)
            {
                size += m_GridLayout.cellSize.x;
            }
            else
            {
                size += LayoutUtility.GetPreferredWidth(item);
            }
            size *= m_Content.localScale.x;
            return size;
        }

        protected override float GetDimension(Vector2 vector)
        {
            return -vector.x;
        }
        
        protected override float GetAbsDimension(Vector2 vector)
        {
            return vector.x;
        }

        protected override Vector2 GetVector(float value)
        {
            return new Vector2(-value, 0);
        }

        protected override void Awake()
        {
            direction = LoopScrollRectDirection.Horizontal;
            base.Awake();
            if (m_Content)
            {
                GridLayoutGroup layout = m_Content.GetComponent<GridLayoutGroup>();
                if (layout != null && layout.constraint != GridLayoutGroup.Constraint.FixedRowCount)
                {
                    Debug.LogError("[LoopScrollRect] unsupported GridLayoutGroup constraint");
                }
            }
        }

        protected override bool UpdateItems(ref Bounds viewBounds, ref Bounds contentBounds)
        {
            bool changed = false;

            // special case: handling move several page in one frame
            if ((viewBounds.size.x < contentBounds.min.x - viewBounds.max.x) && lastItemIndex > startItemIndex)
            {
                float currentSize = contentBounds.size.x;
                float elementSize = (currentSize - contentSpacing * (CurrentLines - 1)) / CurrentLines;
                ReturnToTempPool(false, lastItemIndex - startItemIndex);
                lastItemIndex = startItemIndex;

                int offsetCount = Mathf.FloorToInt((contentBounds.min.x - viewBounds.max.x) / (elementSize + contentSpacing));
                if (totalCount >= 0 && startItemIndex - offsetCount * contentConstraintCount < 0)
                {
                    offsetCount = Mathf.FloorToInt((float)(startItemIndex) / contentConstraintCount);
                }
                startItemIndex -= offsetCount * contentConstraintCount;
                if (totalCount >= 0)
                {
                    startItemIndex = Mathf.Max(startItemIndex, 0);
                }
                lastItemIndex = startItemIndex;

                float offset = offsetCount * (elementSize + contentSpacing);
                m_Content.anchoredPosition -= new Vector2(offset + (reverseDirection ? currentSize : 0), 0);
                contentBounds.center -= new Vector3(offset + currentSize / 2, 0, 0);
                contentBounds.size = Vector3.zero;

                changed = true;
            }

            if ((viewBounds.min.x - contentBounds.max.x > viewBounds.size.x)  && lastItemIndex > startItemIndex)
            {
                int maxItemTypeStart = -1;
                if (totalCount >= 0)
                {
                    maxItemTypeStart = Mathf.Max(0, totalCount - (lastItemIndex - startItemIndex));
                    maxItemTypeStart = (maxItemTypeStart / contentConstraintCount) * contentConstraintCount;
                }
                float currentSize = contentBounds.size.x;
                float elementSize = (currentSize - contentSpacing * (CurrentLines - 1)) / CurrentLines;
                ReturnToTempPool(true, lastItemIndex - startItemIndex);
                // TODO: fix with contentConstraint?
                startItemIndex = lastItemIndex;
            
                int offsetCount = Mathf.FloorToInt((viewBounds.min.x - contentBounds.max.x) / (elementSize + contentSpacing));
                if (maxItemTypeStart >= 0 && startItemIndex + offsetCount * contentConstraintCount > maxItemTypeStart)
                {
                    offsetCount = Mathf.FloorToInt((float)(maxItemTypeStart - startItemIndex) / contentConstraintCount);
                }
                startItemIndex += offsetCount * contentConstraintCount;
                if (totalCount >= 0)
                {
                    startItemIndex = Mathf.Max(startItemIndex, 0);
                }
                lastItemIndex = startItemIndex;

                float offset = offsetCount * (elementSize + contentSpacing);
                m_Content.anchoredPosition += new Vector2(offset + (reverseDirection ? 0 : currentSize), 0);
                contentBounds.center += new Vector3(offset + currentSize / 2, 0, 0);
                contentBounds.size = Vector3.zero;

                changed = true;
            }

            if (viewBounds.max.x < contentBounds.max.x - threshold - m_ContentRightPadding)
            {
                float size = DeleteItemAtEnd(), totalSize = size;
                while (size > 0 && viewBounds.max.x < contentBounds.max.x - threshold - m_ContentRightPadding - totalSize)
                {
                    size = DeleteItemAtEnd();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            if (viewBounds.min.x > contentBounds.min.x + threshold + m_ContentLeftPadding)
            {
                float size = DeleteItemAtStart(), totalSize = size;
                while (size > 0 && viewBounds.min.x > contentBounds.min.x + threshold + m_ContentLeftPadding + totalSize)
                {
                    size = DeleteItemAtStart();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            if (viewBounds.max.x > contentBounds.max.x - m_ContentRightPadding)
            {
                float size = NewItemAtEnd(), totalSize = size;
                while (size > 0 && viewBounds.max.x > contentBounds.max.x - m_ContentRightPadding + totalSize)
                {
                    size = NewItemAtEnd();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            if (viewBounds.min.x < contentBounds.min.x + m_ContentLeftPadding)
            {
                float size = NewItemAtStart(), totalSize = size;
                while (size > 0 && viewBounds.min.x < contentBounds.min.x + m_ContentLeftPadding - totalSize)
                {
                    size = NewItemAtStart();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            if (changed)
            {
                ClearTempPool();
            }

            return changed;
        }
    }
}