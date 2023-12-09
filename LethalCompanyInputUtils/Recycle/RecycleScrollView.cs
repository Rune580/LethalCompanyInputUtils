using UnityEngine;
using UnityEngine.UI;

namespace LethalCompanyInputUtils.Recycle;

public abstract class RecycleScrollView<TItem, TItemData> : MonoBehaviour
    where TItem : RecycleViewItem<TItemData>
{
    public GameObject? itemPrefab;
    public ScrollRect? targetScrollRect;
    public RectOffset? padding;
    public float spacing;
    public int maxPoolSize = 10;
    public float coverageMultiplier = 1.5f;
    public float recyclingThreshold = 0.2f;
    
    private readonly RecycleArray<TItem> _pool = [];
    private readonly Vector3[] _corners = new Vector3[4];
    
    private RectTransform? _viewport;
    private RectTransform? _content;
    private Vector2 _prevAnchoredPos;
    private Bounds _viewBounds;
    private bool _recycling;

    private void Awake()
    {
        if (!itemPrefab)
            return;
        if (!targetScrollRect)
            return;

        _viewport = targetScrollRect!.viewport;
        if (!_viewport)
            return;
        
        UpdateViewBounds();

        _content = targetScrollRect.content;
        if (!_content)
            return;

        _content.drivenByObject = this;
        _content.drivenProperties = DrivenTransformProperties.AnchorMin | DrivenTransformProperties.AnchorMax |
                                    DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.Pivot |
                                    DrivenTransformProperties.SizeDelta;
        _content.anchorMin = new Vector2(0, 1);
        _content.anchorMax = new Vector2(1, 1);
        _content.pivot = new Vector2(0.5f, 1);

        _prevAnchoredPos = _content.anchoredPosition;
        
        targetScrollRect.onValueChanged.AddListener(HandleScroll);
    }
    
    protected abstract int GetDataLength();

    protected abstract TItemData GetData(int index);

    protected void UpdateContentSize()
    {
        int length = GetDataLength();
        
        float itemHeight = itemPrefab!.GetComponent<RectTransform>().rect.size.y;
        float contentHeight = spacing * (length - 1) + itemHeight * length;

        _content!.sizeDelta = new Vector2(_content.sizeDelta.x, contentHeight + padding!.bottom + padding.top);
    }

    private void UpdateViewBounds()
    {
        _viewport!.GetWorldCorners(_corners);
        
        float threshold = recyclingThreshold * (_corners[2].y - _corners[0].y);
        _viewBounds.min = new Vector3(_corners[0].x, _corners[0].y - threshold);
        _viewBounds.max = new Vector3(_corners[2].x, _corners[2].y + threshold);
    }

    protected void SetupPool()
    {
        _pool.Clear();
        float length = GetDataLength();

        float requiredCoverage = coverageMultiplier * _viewport!.rect.height;
        int minPoolSize = (int)Mathf.Min(length, maxPoolSize);

        float currentCoverage = 0;
        float posY = -padding!.top;

        while ((_pool.Size < minPoolSize || currentCoverage < requiredCoverage) && _pool.Size < length)
        {
            TItem item = Instantiate(itemPrefab)!.GetComponent<TItem>();
            item.gameObject.name = $"RecycleViewItem {_pool.Size + 1}";

            RectTransform itemTransform = item.rectTransform;
            itemTransform.anchoredPosition = new Vector2(padding.left, posY);

            float itemHeight = itemTransform.rect.height;
            posY = (itemTransform.anchoredPosition.y - itemHeight) - spacing;
            currentCoverage += itemHeight + spacing;

            itemTransform.sizeDelta = new Vector2(-(padding.left + padding.right), itemHeight);
            
            item.BindData(GetData(_pool.Size));
            _pool.Add(item);
        }
    }
    
    protected void RefreshView()
    {
        int i = _pool.TopDataIndex;
        foreach (var item in _pool)
        {
            item.BindData(GetData(i));
            i++;
        }
    }

    private void HandleScroll(Vector2 _)
    {
        Vector2 dir = _content!.anchoredPosition - _prevAnchoredPos;
        
        RecycleOnScroll(dir);

        _prevAnchoredPos = _content.anchoredPosition;
    }

    private void RecycleOnScroll(Vector2 dir)
    {
        if (_recycling)
            return;
        
        UpdateViewBounds();

        if (dir.y > 0 && GetItemMaxY(_pool.GetBottom()) > _viewBounds.min.y)
        {
            RecycleTopToBottom();
        }
        else if (dir.y < 0 && GetItemMinY(_pool.GetTop()) < _viewBounds.max.y)
        {
            RecycleBottomToTop();
        }
    }

    private void RecycleTopToBottom()
    {
        _recycling = true;

        while (GetItemMinY(_pool.GetTop()) > _viewBounds.max.y && _pool.BottomDataIndex + 1 < GetDataLength())
        {
            MoveItemBelowOther(_pool.GetTop(), _pool.GetBottom());
            
            _pool.RecycleTopToBottom().BindData(GetData(_pool.BottomDataIndex));
        }

        _recycling = false;
    }

    private void RecycleBottomToTop()
    {
        _recycling = true;

        while (GetItemMaxY(_pool.GetBottom()) < _viewBounds.min.y && _pool.BottomDataIndex + 1 > _pool.Size)
        {
            MoveItemAboveOther(_pool.GetBottom(), _pool.GetTop());
            
            _pool.RecycleBottomToTop().BindData(GetData(_pool.TopDataIndex));
        }

        _recycling = false;
    }

    private void MoveItemBelowOther(TItem item, TItem other)
    {
        RectTransform itemRectTransform = item.rectTransform;
        RectTransform otherRectTransform = other.rectTransform;

        float posY = otherRectTransform.anchoredPosition.y - (otherRectTransform.sizeDelta.y + spacing);

        itemRectTransform.anchoredPosition = new Vector2(itemRectTransform.anchoredPosition.x, posY);
    }

    private void MoveItemAboveOther(TItem item, TItem other)
    {
        RectTransform itemRectTransform = item.rectTransform;
        RectTransform otherRectTransform = other.rectTransform;
        
        float posY = otherRectTransform.anchoredPosition.y + otherRectTransform.sizeDelta.y + spacing;
        
        itemRectTransform.anchoredPosition = new Vector2(itemRectTransform.anchoredPosition.x, posY);
    }

    private float GetItemMaxY(TItem item)
    {
        item.rectTransform.GetWorldCorners(_corners);
        return _corners[1].y;
    }

    private float GetItemMinY(TItem item)
    {
        item.rectTransform.GetWorldCorners(_corners);
        return _corners[0].y;
    }
}