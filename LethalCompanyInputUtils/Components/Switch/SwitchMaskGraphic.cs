using LethalCompanyInputUtils.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace LethalCompanyInputUtils.Components.Switch;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class SwitchMaskGraphic : Graphic
{
    public float width;
    public float height;

    public Vector2 Offset
    {
        get => _offset;
        set
        {
            _offset = value;
            UpdateGeometry();
        }
    }

    private Vector2 _offset = new(-45, 0);
    private RectTransform? _rectTransform;

    public override void Awake()
    {
        base.Awake();

        if (_rectTransform is null)
            _rectTransform = GetComponent<RectTransform>();
    }

    private void OnValidate()
    {
        UpdateGeometry();
    }

    public override void OnPopulateMesh(VertexHelper vh)
    {
        if (_rectTransform is null)
            return;
        
        var vertColor = new Color32(255, 255, 255, 255);
        
        var center = _rectTransform.rect.CenteredPos();
        
        vh.Clear();
        
        // var tl = new Vector2(center.x - width / 2f, center.y + height / 2f) + offset;
        // var tr = new Vector2(center.x + width / 2f, center.y + height / 2f) + offset;
        // var bl = new Vector2(center.x - width / 2f, center.y - height / 2f) + offset;
        // var br = new Vector2(center.x + width / 2f, center.y - height / 2f) + offset;
        
        var tl = new Vector2( - width / 2f, height / 2f) + Offset;
        var tr = new Vector2(width / 2f, height / 2f) + Offset;
        var bl = new Vector2(- width / 2f, - height / 2f) + Offset;
        var br = new Vector2(width / 2f, - height / 2f) + Offset;
        
        vh.AddUIVertexQuad([
            new UIVertex { position = tl, color = vertColor },
            new UIVertex { position = tr, color = vertColor },
            new UIVertex { position = br, color = vertColor },
            new UIVertex { position = bl, color = vertColor }
        ]);
    }
}