using System;
using System.IO;
using LethalCompanyInputUtils.Utils;

namespace LethalCompanyInputUtils.Config;

public enum BindingOverrideType
{
    Global,
    Local
}

public static class BindingOverrideTypeExtensions
{
    public static string GetJsonPath(this BindingOverrideType overrideType, string name)
    {
        var basePath = overrideType switch
        {
            BindingOverrideType.Global => FsUtils.PersistentControlsDir,
            BindingOverrideType.Local => FsUtils.ControlsDir,
            _ => throw new ArgumentOutOfRangeException(nameof(overrideType), overrideType, null)
        };

        return Path.Combine(basePath, $"{name}.json");
    }
}