using UnityEngine;

namespace ReuseScrollRect.Runtime
{
    // optional class for better scroll support
    public interface IItemSize
    {
        Vector2 GetItemsSize(int itemsCount);
    }
}
