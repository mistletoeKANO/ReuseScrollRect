using UnityEngine;
using UnityEngine.UI;

namespace ReuseScrollRect.Runtime
{
    [AddComponentMenu("UI/Reuse Vertical Scroll Rect", 51)]
    [DisallowMultipleComponent]
    public class RVerticalScrollRect : RScrollRect
    {
        protected override float GetSize(RectTransform item, bool includeSpacing)
        {
            float size = includeSpacing ? contentSpacing : 0;
            if (m_GridLayout != null)
            {
                size += m_GridLayout.cellSize.y;
            }
            else
            {
                size += LayoutUtility.GetPreferredHeight(item);
            }
            size *= m_Content.localScale.y;
            return size;
        }

        protected override float GetDimension(Vector2 vector)
        {
            return vector.y;
        }
        
        protected override float GetAbsDimension(Vector2 vector)
        {
            return vector.y;
        }

        protected override Vector2 GetVector(float value)
        {
            return new Vector2(0, value);
        }

        protected override void Awake()
        {
            direction = LoopScrollRectDirection.Vertical;
            base.Awake();
            if (m_Content)
            {
                GridLayoutGroup layout = m_Content.GetComponent<GridLayoutGroup>();
                if (layout != null && layout.constraint != GridLayoutGroup.Constraint.FixedColumnCount)
                {
                    Debug.LogError("[LoopScrollRect] unsupported GridLayoutGroup constraint");
                }
            }
        }

        protected override bool UpdateItems(ref Bounds viewBounds, ref Bounds contentBounds)
        {
            bool changed = false;

            // special case: handling move several page in one frame
            if ((viewBounds.size.y < contentBounds.min.y - viewBounds.max.y) && lastItemIndex > startItemIndex)
            {
                int maxItemTypeStart = -1;
                if (totalCount >= 0)
                {
                    maxItemTypeStart = Mathf.Max(0, totalCount - (lastItemIndex - startItemIndex));
                }
                float currentSize = contentBounds.size.y;
                float elementSize = (currentSize - contentSpacing * (CurrentLines - 1)) / CurrentLines;
                ReturnToTempPool(true, lastItemIndex - startItemIndex);
                startItemIndex = lastItemIndex;

                int offsetCount = Mathf.FloorToInt((contentBounds.min.y - viewBounds.max.y) / (elementSize + contentSpacing));
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
                m_Content.anchoredPosition -= new Vector2(0, offset + (reverseDirection ? 0 : currentSize));
                contentBounds.center -= new Vector3(0, offset + currentSize / 2, 0);
                contentBounds.size = Vector3.zero;

                changed = true;
            }

            if ((viewBounds.min.y - contentBounds.max.y > viewBounds.size.y) && lastItemIndex > startItemIndex)
            {
                float currentSize = contentBounds.size.y;
                float elementSize = (currentSize - contentSpacing * (CurrentLines - 1)) / CurrentLines;
                ReturnToTempPool(false, lastItemIndex - startItemIndex);
                lastItemIndex = startItemIndex;

                int offsetCount = Mathf.FloorToInt((viewBounds.min.y - contentBounds.max.y) / (elementSize + contentSpacing));
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
                m_Content.anchoredPosition += new Vector2(0, offset + (reverseDirection ? currentSize : 0));
                contentBounds.center += new Vector3(0, offset + currentSize / 2, 0);
                contentBounds.size = Vector3.zero;

                changed = true;
            }

            if (viewBounds.min.y > contentBounds.min.y + threshold + m_ContentBottomPadding)
            {
                float size = DeleteItemAtEnd(), totalSize = size;
                while (size > 0 && viewBounds.min.y > contentBounds.min.y + threshold + m_ContentBottomPadding + totalSize)
                {
                    size = DeleteItemAtEnd();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            if (viewBounds.max.y < contentBounds.max.y - threshold - m_ContentTopPadding)
            {
                float size = DeleteItemAtStart(), totalSize = size;
                while (size > 0 && viewBounds.max.y < contentBounds.max.y - threshold - m_ContentTopPadding - totalSize)
                {
                    size = DeleteItemAtStart();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            if (viewBounds.min.y < contentBounds.min.y + m_ContentBottomPadding)
            {
                float size = NewItemAtEnd(), totalSize = size;
                while (size > 0 && viewBounds.min.y < contentBounds.min.y + m_ContentBottomPadding - totalSize)
                {
                    size = NewItemAtEnd();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            if (viewBounds.max.y > contentBounds.max.y - m_ContentTopPadding)
            {
                float size = NewItemAtStart(), totalSize = size;
                while (size > 0 && viewBounds.max.y > contentBounds.max.y - m_ContentTopPadding + totalSize)
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