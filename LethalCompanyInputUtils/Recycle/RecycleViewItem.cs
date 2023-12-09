using UnityEngine;

namespace LethalCompanyInputUtils.Recycle;

public abstract class RecycleViewItem<TItemData> : MonoBehaviour
{
    [HideInInspector]
    public RectTransform? rectTransform;

    protected TItemData? BoundData;

    protected void Awake()
    {
        if (!rectTransform)
            rectTransform = GetComponent<RectTransform>();

        if (BoundData is null)
            return;
        
        ResetState();
        ReloadData();
    }

    protected void OnEnable()
    {
        if (!rectTransform)
            rectTransform = GetComponent<RectTransform>();

        if (BoundData is null)
            return;
        
        ResetState();
        ReloadData();
    }

    public void BindData(TItemData data)
    {
        BoundData = data;
        
        ResetState();
        ReloadData();
    }

    protected abstract void ReloadData();

    protected abstract void ResetState();
}