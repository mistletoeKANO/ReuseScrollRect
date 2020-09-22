using System.Collections.Generic;

namespace UnityEngine.UI
{
    public sealed class ReuseScrollRect : ScrollRect
    {
        /// <summary>
        /// 相关联的 item 对象
        /// </summary>
        [SerializeField] private GameObject m_Item;
        public GameObject Item
        {
            get => m_Item;
            set => m_Item = value;
        }

        /// <summary>
        /// 起始 item 距离 边框的距离
        /// </summary>
        [SerializeField] private Vector2 m_Padding;

        public Vector2 Padding
        {
            get { return m_Padding; }
            set { m_Padding = value; }
        }

        /// <summary>
        /// item 之间 的空隙
        /// </summary>
        [SerializeField] private Vector2 m_Spacing;

        public Vector2 Spacing
        {
            get { return m_Spacing; }
            set { m_Spacing = value; }
        }

        private int cellCount;
        private float cellWidth;
        private float cellHeight;
        private List<System.Action<ReuseScrollItem>> onCellUpdateList = new List<System.Action<ReuseScrollItem>>();

        private int row;
        private int col;

        private bool inited;
        private List<GameObject> cellList = new List<GameObject>();

        public void AddCellListener(System.Action<ReuseScrollItem> call)
        {
            this.onCellUpdateList.Add(call);
            this.RefreshAllCells();
        }

        public void RemoveCellListener(System.Action<ReuseScrollItem> call)
        {
            this.onCellUpdateList.Remove(call);
        }

        public void SetCellCount(int count)
        {
            this.cellCount = Mathf.Max(0, count);

            if (this.inited == false)
            {
                this.Init();
            }

            if (this.horizontal)
            {
                float newContentWidth = this.cellWidth * Mathf.CeilToInt((float) this.cellCount / this.row);
                float newMaxX = newContentWidth - this.viewport.rect.width + m_Padding.x * 2; //当minX==0时maxX的位置
                float minX = this.content.offsetMin.x;
                newMaxX += minX;
                newMaxX = Mathf.Max(minX, newMaxX);
                this.content.offsetMax = new Vector2(newMaxX, 0);
            }
            else
            {
                float newContentHeight = this.cellHeight * Mathf.CeilToInt((float)cellCount / this.col);
                float newMinY = -newContentHeight + this.viewport.rect.height - m_Padding.y * 2;
                float maxY = this.content.offsetMax.y;
                newMinY += maxY;//保持位置
                newMinY = Mathf.Min(maxY, newMinY);//保证不小于viewport的高度。
                this.content.offsetMin = new Vector2(0, newMinY);
            }

            
            this.CreateCells();
        }

        public void Init()
        {
            if (m_Item == null)
            {
                Debug.LogError("Item 不能为空！");
                return;
            }

            this.inited = true;
            this.m_Item.SetActive(false);

            GameObject viewport = null;
            if (transform.Find("Viewport") == null)
            {
                viewport = new GameObject("Viewport", typeof(RectTransform));
            }
            else
            {
                viewport = transform.Find("Viewport").gameObject;
            }

            viewport.transform.SetParent(transform);
            this.viewport = viewport.GetComponent<RectTransform>();
            GameObject content = null;
            if (viewport.transform.Find("Content") == null)
            {
                content = new GameObject("Content", typeof(RectTransform));
            }
            else
            {
                content = viewport.transform.Find("Content").gameObject;
            }

            content.transform.SetParent(viewport.transform);
            this.content = content.GetComponent<RectTransform>();

            this.viewport.localScale = Vector3.one;
            this.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
            this.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
            this.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 0);
            this.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);
            this.viewport.anchorMin = Vector2.zero;
            this.viewport.anchorMax = Vector2.one;

            this.viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            Image image = this.viewport.gameObject.AddComponent<Image>();
            Rect viewRect = this.viewport.rect;
            image.sprite = Sprite.Create(new Texture2D(1, 1), new Rect(Vector2.zero, Vector2.one), Vector2.zero);
            Rect tempRect = m_Item.GetComponent<RectTransform>().rect;
            this.cellWidth = tempRect.width + m_Spacing.x;
            this.cellHeight = tempRect.height + m_Spacing.y;

            if (this.horizontal)
            {
                this.row = Mathf.FloorToInt(viewRect.height / this.cellHeight);
                this.row = Mathf.Max(1, this.row);
                this.col = Mathf.CeilToInt(viewRect.width / this.cellWidth);
            }
            else
            {
                this.col = (int) (viewRect.width / this.cellWidth);
                this.col = Mathf.Max(1, this.col);
                this.row = Mathf.CeilToInt(viewRect.height / this.cellHeight);
            }

            this.content.localScale = Vector3.one;
            this.content.offsetMax = new Vector2(0, 0);
            this.content.offsetMin = new Vector2(0, 0);
            this.content.anchorMin = Vector2.zero;
            this.content.anchorMax = Vector2.one;
            this.onValueChanged.AddListener(this.OnValueChange);
        }

        public void RefreshAllCells()
        {
            foreach (GameObject cell in this.cellList)
            {
                this.cellUpdate(cell);
            }
        }

        private void CreateCells()
        {
            if (this.horizontal)
            {
                for (int r = 0; r < this.row; r++)
                {
                    for (int l = 0; l < this.col + 1; l++)
                    {
                        int index = r * (this.col + 1) + l;
                        if (index < this.cellCount)
                        {
                            CreateItem(l, r, index);
                        }
                    }
                }
            }
            else
            {
                for (int r = 0; r < this.row + 1; r++)
                {
                    for (int l = 0; l < this.col; l++)
                    {
                        int index = r * this.col + l;
                        if (index < this.cellCount)
                        {
                            CreateItem(l, r, index);
                        }
                    }
                }
            }

            this.RefreshAllCells();
        }

        private void CreateItem(int l, int r, int index)
        {
            if (this.cellList.Count > index)
            {
                return;
            }

            GameObject newcell = GameObject.Instantiate<GameObject>(this.m_Item);
            newcell.SetActive(true);
            RectTransform cellRect = newcell.GetComponent<RectTransform>();
            cellRect.anchorMin = new Vector2(0, 1);
            cellRect.anchorMax = new Vector2(0, 1);

            float x = this.cellWidth / 2 + l * this.cellWidth + m_Padding.x;
            float y = -r * this.cellHeight - this.cellHeight / 2 - m_Padding.y;
            cellRect.SetParent(this.content);
            cellRect.localScale = Vector3.one;
            cellRect.anchoredPosition = new Vector3(x, y);
            newcell.AddComponent<ReuseScrollItem>().SetObjIndex(index);
            this.cellList.Add(newcell);
        }

        private void OnValueChange(Vector2 pos)
        {
            foreach (GameObject cell in this.cellList)
            {
                if (this.horizontal)
                {
                    OnValueChangeHor(cell);
                }
                else
                {
                    OnValueChangeVer(cell);
                }
            }
        }

        private void OnValueChangeHor(GameObject cell)
        {
            RectTransform cellRect = cell.GetComponent<RectTransform>();
            float dist = this.content.offsetMin.x + cellRect.anchoredPosition.x;
            float minLeft = -this.cellWidth / 2;
            float maxRight = this.col * this.cellWidth + this.cellWidth / 2;
            //限定复用边界
            if (dist < minLeft)
            {
                //控制cell的anchoredPosition在content的范围内才重复利用。
                float newX = cellRect.anchoredPosition.x + (this.col + 1) * (this.cellWidth);
                if (newX < this.content.rect.width)
                {
                    cellRect.anchoredPosition = new Vector3(newX, cellRect.anchoredPosition.y);
                    this.cellUpdate(cell);
                }
            }
            else if (dist > maxRight)
            {
                float newX = cellRect.anchoredPosition.x - (this.col + 1) * (this.cellWidth);
                if (newX > 0)
                {
                    cellRect.anchoredPosition = new Vector3(newX, cellRect.anchoredPosition.y);
                    this.cellUpdate(cell);
                }
            }
        }

        private void OnValueChangeVer(GameObject cell)
        {
            RectTransform cellRect = cell.GetComponent<RectTransform>();
            float dist = this.content.offsetMax.y + cellRect.anchoredPosition.y;
            float maxTop = this.cellHeight / 2;
            float minBottom = -((this.row + 1) * this.cellHeight) + this.cellHeight / 2;
            if (dist > maxTop)
            {
                float newY = cellRect.anchoredPosition.y - (this.row + 1) * this.cellHeight;
                //保证cell的anchoredPosition只在content的高的范围内活动，下同理
                if (newY > -this.content.rect.height)
                {
                    //重复利用cell，重置位置到视野范围内。
                    cellRect.anchoredPosition = new Vector3(cellRect.anchoredPosition.x, newY);
                    this.cellUpdate(cell);
                }
            }
            else if (dist < minBottom)
            {
                float newY = cellRect.anchoredPosition.y + (this.row + 1) * this.cellHeight;
                if (newY < 0)
                {
                    cellRect.anchoredPosition = new Vector3(cellRect.anchoredPosition.x, newY);
                    this.cellUpdate(cell);
                }
            }
        }

        private int allCol
        {
            get { return Mathf.CeilToInt((float) this.cellCount / this.row); }
        }

        private int allRow
        {
            get { return Mathf.CeilToInt((float) this.cellCount / this.col); }
        }

        private void cellUpdate(GameObject cell)
        {
            RectTransform cellRect = cell.GetComponent<RectTransform>();
            int x = Mathf.CeilToInt((cellRect.anchoredPosition.x - cellWidth / 2 - m_Padding.x) / cellWidth);
            int y = Mathf.Abs(Mathf.CeilToInt((cellRect.anchoredPosition.y + cellHeight / 2) / cellHeight));

            int index = 0;
            if (this.horizontal)
            {
                index = y * allCol + x;
            }
            else
            {
                index = y * this.col + x;
            }

            ReuseScrollItem scrollGridCell = cell.GetComponent<ReuseScrollItem>();
            scrollGridCell.UpdatePos(x, y, index);
            if (index >= cellCount || x >= (this.horizontal ? this.allCol : this.allRow))
            {
                cell.SetActive(false);
            }
            else
            {
                if (cell.activeSelf == false)
                {
                    cell.SetActive(true);
                }

                foreach (var call in this.onCellUpdateList)
                {
                    call(scrollGridCell);
                }
            }

        }
    }
}