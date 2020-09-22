using UnityEditor;

namespace UnityEngine.UI
{
    public class SlMOUIEx
    {
        [MenuItem("GameObject/SLMOUI/ReuseScrollRect",false,10)]
        static void AddSlMoReuseScrollRect(MenuCommand menuCommand)
        {
            var curSelectTrans = Selection.gameObjects[0].transform;
            var newObj = new GameObject("ReuseScrollRect",typeof(RectTransform));
            newObj.transform.SetParent(curSelectTrans);
            newObj.transform.localPosition = Vector2.zero;
            newObj.AddComponent<CanvasRenderer>();
            var color = newObj.AddComponent<Image>();
            color.color = new Color(0.29f, 0.29f, 0.29f, 0.26f);
            newObj.AddComponent<ReuseScrollRect>();
            Selection.activeObject = newObj;
        }
    }
}