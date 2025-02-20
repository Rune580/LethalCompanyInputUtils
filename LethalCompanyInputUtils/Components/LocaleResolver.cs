using LethalCompanyInputUtils.Localization;
using TMPro;
using UnityEngine;

namespace LethalCompanyInputUtils.Components;

public class LocaleResolver : MonoBehaviour
{
    [TextArea(3, 20)]
    public string token = "";
    public bool getTokenFromLabel;
    public TextMeshProUGUI? label;

    public void Awake()
    {
        if (getTokenFromLabel)
            return;
        
        SetLabel();
    }

    private void Start()
    {
        if (getTokenFromLabel)
            return;
        
        SetLabel();
    }

    private void OnEnable()
    {
        if (getTokenFromLabel)
            return;
        
        SetLabel();
    }

    private void Update()
    {
        // Hacky solution for the dropdown locale tokens, because I'm lazy and don't feel like making a custom dropdown component to handle locale.
        if (!getTokenFromLabel)
            return;
        
        SetLabel();
    }

    public void ReloadFromLabel()
    {
        getTokenFromLabel = true;
        SetLabel();
    }

    private void SetLabel()
    {
        if (label is null)
            return;

        if (getTokenFromLabel)
        {
            token = label.text;
            if (!LocaleManager.ContainsToken(token))
                return;
            
            getTokenFromLabel = false;
        }
        
        label.SetText(LocaleManager.GetString(token));
    }
}