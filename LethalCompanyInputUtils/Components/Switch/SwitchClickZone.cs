using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LethalCompanyInputUtils.Components.Switch;

public class SwitchClickZone : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent onClick = new();
    
    public void OnPointerClick(PointerEventData eventData) => onClick.Invoke();
}