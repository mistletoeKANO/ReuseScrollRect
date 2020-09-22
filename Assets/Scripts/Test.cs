using TMPro;

namespace UnityEngine.UI
{
    public class Test : MonoBehaviour
    {
        void Start()
        {
            ReuseScrollRect scrollGridVertical = gameObject.GetComponent<ReuseScrollRect>();
            scrollGridVertical.AddCellListener(this.OnCellUpdate);
            scrollGridVertical.SetCellCount(153);
        }

        private void OnCellUpdate(ReuseScrollItem cell)
        {
            cell.gameObject.GetComponentInChildren<Text>().text = cell.index.ToString();
        }
    }
}