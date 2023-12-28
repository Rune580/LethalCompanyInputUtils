using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;

namespace LethalCompanyInputUtils.Utils;

internal static class AssemblyUtils
{
    public static BepInPlugin? GetBepInPlugin(this Assembly assembly)
    {
        foreach (var type in assembly.GetValidTypes())
        {
            var plugin = type.GetCustomAttribute<BepInPlugin>();
            if (plugin is not null)
                return plugin;
        }

        return null;
    }

    public static IEnumerable<Type> GetValidTypes(this Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(type => type is not null);
        }
    }
}