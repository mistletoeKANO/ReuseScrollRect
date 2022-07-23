using ReuseScrollRect.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ReuseScrollRect.Editor
{
    public static class RDefaultControls
    {
        private static readonly Color s_PanelColor = new Color(1f, 1f, 1f, 0.392f);
        private static GameObject CreateUIElementRoot(string name, Vector2 size, params System.Type[] components)
        {
            GameObject child = new GameObject(name, components);
            RectTransform rectTransform = child.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = size;
            return child;
        }

        static GameObject CreateUIObject(string name, GameObject parent, params System.Type[] components)
        {
            GameObject go = new GameObject(name, components);
            SetParentAndAlign(go, parent);
            return go;
        }

        private static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null)
                return;

            child.transform.SetParent(parent.transform, false);
            SetLayerRecursively(child, parent.layer);
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }

        public static GameObject CreateReuseScrollRect(int axis)
        {
            return CreateReuseScrollRectInternal(axis);
        }
        static GameObject CreateReuseScrollRectInternal(int axis)
        {
            var rootName = axis == 0 ? "Reuse Horizontal Scroll Rect" : "Reuse Vertical Scroll Rect";
            var type = axis == 0 ? typeof(RHorizontalScrollRect) : typeof(RVerticalScrollRect);
            GameObject root = CreateUIElementRoot(rootName, new Vector2(200, 200), typeof(Image), type);
            GameObject viewport = CreateUIObject("Viewport", root, typeof(Image), typeof(Mask));
            GameObject content = CreateUIObject("Content", viewport, typeof(RectTransform));
            
            // Make viewport fill entire scroll view.
            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = Vector2.zero;
            viewportRT.pivot = Vector2.up;

            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = axis == 0? new Vector2(0, 0) : new Vector2(0, 1);
            contentRT.anchorMax = axis == 0? new Vector2(0, 1) : new Vector2(1, 1);
            contentRT.sizeDelta = Vector2.zero;
            contentRT.pivot = axis == 0? new Vector2(0, 0.5f) : new Vector2(0.5f, 1);

            RScrollRect scrollRect;
            if (axis == 0)
                scrollRect = root.GetComponent<RHorizontalScrollRect>();
            else
                scrollRect = root.GetComponent<RVerticalScrollRect>();
            scrollRect.content = contentRT;
            scrollRect.viewport = viewportRT;
            scrollRect.horizontalScrollbar = null;
            scrollRect.verticalScrollbar = null;
            scrollRect.horizontal = axis == 0;
            scrollRect.vertical = axis == 1;
            scrollRect.horizontalScrollbarVisibility = RScrollRectBase.ScrollbarVisibility.Permanent;
            scrollRect.verticalScrollbarVisibility = RScrollRectBase.ScrollbarVisibility.Permanent;
            scrollRect.horizontalScrollbarSpacing = 0;
            scrollRect.verticalScrollbarSpacing = 0;
            
            // Setup UI components.
            Image rootImg = root.GetComponent<Image>();
            rootImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            rootImg.type = Image.Type.Sliced;
            rootImg.color = s_PanelColor;
            
            Image viewportImage = viewport.GetComponent<Image>();
            viewportImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
            viewportImage.type = Image.Type.Sliced;

            HorizontalOrVerticalLayoutGroup layoutGroup;
            if (axis == 0)
                layoutGroup = content.AddComponent<HorizontalLayoutGroup>();
            else
                layoutGroup = content.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = axis == 0 ? TextAnchor.MiddleLeft : TextAnchor.UpperCenter;
            layoutGroup.childForceExpandWidth = axis == 1;
            layoutGroup.childForceExpandHeight = axis == 0;

            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = axis == 0 ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
            sizeFitter.verticalFit = axis == 0? ContentSizeFitter.FitMode.Unconstrained : ContentSizeFitter.FitMode.PreferredSize;

            return root;
        }
    }
}
