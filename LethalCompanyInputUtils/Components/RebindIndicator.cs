using System;
using TMPro;
using UnityEngine;

namespace LethalCompanyInputUtils.Components;

public class RebindIndicator : MonoBehaviour
{
    public TextMeshProUGUI? label;
    public int maxTicks = 5;
    public float timeBetweenTicks = 1f;
    
    private int _ticks = 1;
    private float _timer;

    private void OnEnable()
    {
        _ticks = 1;
        _timer = timeBetweenTicks;
        label!.SetText(GetText());
    }

    private void OnDisable()
    {
        _ticks = 1;
        _timer = timeBetweenTicks;
        label!.SetText("");
    }

    private void Update()
    {
        _timer -= Time.unscaledDeltaTime;

        if (_timer > 0f)
            return;

        _ticks++;
        if (_ticks > maxTicks)
            _ticks = 1;
        
        label!.SetText(GetText());

        _timer = timeBetweenTicks;
    }

    private string GetText()
    {
        string text = "";

        for (int i = 0; i < _ticks; i++)
            text += ".";

        return text;
    }
}