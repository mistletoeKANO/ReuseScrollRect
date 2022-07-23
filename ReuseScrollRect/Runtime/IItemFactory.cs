using UnityEngine;

namespace ReuseScrollRect.Runtime
{
    public interface IItemFactory
    {
        /// <summary>
        /// instantiate item when it sliding in viewport
        /// </summary>
        /// <param name="index">item index</param>
        /// <returns></returns>
        GameObject InstantiateOneItem(int index);
        /// <summary>
        /// recycle item, when it sliding out viewport
        /// </summary>
        /// <param name="trans">item</param>
        void RecycleOneItem(Transform trans);
        /// <summary>
        /// Refresh item when item scroll in viewport
        /// </summary>
        /// <param name="transform">item's transform</param>
        /// <param name="index">item index in item list</param>
        void ItemRefreshEvent(Transform transform, int index);
    }
}
