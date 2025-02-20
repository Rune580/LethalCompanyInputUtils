using UnityEngine;
using UnityEngine.EventSystems;

namespace LethalCompanyInputUtils.Components.PopOvers;

[RequireComponent(typeof(RectTransform))]
public class PopOverTrigger : UIBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea(3, 20)]
    public string text = "";
    
    public bool textIsLangToken;
    
    public PopOver.Placement placement;

    public RectTransform? target;

    public override void Awake()
    {
        base.Awake();

        if (target is null)
            target = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PopOverManager.AddHotTrigger(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PopOverManager.RemoveHotTrigger(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        
        PopOverManager.RemoveHotTrigger(this);
    }
}