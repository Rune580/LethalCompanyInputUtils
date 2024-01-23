using System.Collections.Generic;
using System.Linq;
using LethalCompanyInputUtils.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Glyphs;

[CreateAssetMenu]
public class ControllerGlyph : ScriptableObject
{
    public List<string> validGamepadTypes = [];

    public List<GlyphDef> glyphSet = [];

    private readonly Dictionary<string, Sprite?> _glyphLut = new();
    
    private static readonly List<ControllerGlyph> Instances = [];
    
    private static bool _loaded;

    public static ControllerGlyph? MouseGlyphs { get; private set; }

    public static ControllerGlyph? GetBestMatching()
    {
        if (Instances.Count == 0)
            return null;
        
        foreach (var controllerGlyph in Instances)
        {
            if (controllerGlyph.IsCurrent)
                return controllerGlyph;
        }

        return Instances[0];
    }

    internal static void LoadGlyphs()
    {
        if (_loaded)
            return;
        
        Assets.Load<ControllerGlyph>("controller glyphs/xbox series x glyphs.asset");
        Assets.Load<ControllerGlyph>("controller glyphs/dualsense glyphs.asset");
        
        MouseGlyphs = Assets.Load<ControllerGlyph>("controller glyphs/mouse glyphs.asset");
        
        _loaded = true;
    }

    public bool IsCurrent
    {
        get
        {
            var current = Gamepad.current;
            if (current is null)
                return false;

            return validGamepadTypes.Any(gamepadTypeName => string.Equals(gamepadTypeName, current.GetType().Name));
        }
    }

    public Sprite? this[string controlPath]
    {
        get
        {
            if (_glyphLut.Count == 0)
                UpdateLut();
            
            return _glyphLut.GetValueOrDefault(controlPath, null);
        }
    }

    private void Awake()
    {
        if (!Instances.Contains(this))
            Instances.Add(this);
    }

    private void UpdateLut()
    {
        foreach (var glyphDef in glyphSet)
            _glyphLut[glyphDef.controlPath] = glyphDef.glyphSprite;
    }

    private void OnDestroy()
    {
        if (Instances.Contains(this))
            Instances.Remove(this);
    }
}