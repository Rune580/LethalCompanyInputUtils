using System;

namespace LethalCompanyInputUtils.SourceGen.Schema;

[Serializable]
public class Control
{
    public string Name { get; set; } = "";

    public string DisplayName { get; set; } = "";
    
    public string? SourceLayout { get; set; }
}