using System;
using System.Collections.Generic;

namespace LethalCompanyInputUtils.Utils;

internal static class EnumUtils
{
    public static IEnumerable<TEnum> EnumerateValues<TEnum>()
        where TEnum : Enum
    {
        var values = Enum.GetValues(typeof(TEnum));

        foreach (var value in values)
        {
            yield return (TEnum)value;
        }
    }
}