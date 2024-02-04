using System;

namespace LethalCompanyInputUtils.Data;

[Serializable]
public struct BindingOverride
{
    public string? action;
    public string? origPath;
    public string? path;
    public string? groups;
}