using System.Reflection;
using BepInEx;

namespace LethalCompanyInputUtils.Utils;

internal static class AssemblyUtils
{
    public static BepInPlugin? GetBepInPlugin(this Assembly assembly)
    {
        foreach (var type in assembly.GetExportedTypes())
        {
            var plugin = type.GetCustomAttribute<BepInPlugin>();
            if (plugin is not null)
                return plugin;
        }

        return null;
    }
}