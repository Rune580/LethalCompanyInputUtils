using UnityEngine;

namespace LethalCompanyInputUtils.Glyphs;

[CreateAssetMenu]
public class GlyphDef : ScriptableObject
{
    public string controlPath = "";
    public Sprite? glyphSprite;
}