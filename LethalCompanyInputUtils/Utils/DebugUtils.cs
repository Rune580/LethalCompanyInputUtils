using System.Collections.Generic;
using System.Text;

namespace LethalCompanyInputUtils.Utils;

internal static class DebugUtils
{
    public static string ToPrettyString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        var builder = new StringBuilder();
        builder.AppendLine("{");
        
        foreach (var (key, value) in dictionary)
            builder.AppendLine($"\t\"{key?.ToString()}\": \"{value?.ToString()}\",");

        builder.Remove(builder.Length - 1, 1);
        builder.AppendLine("}");
        
        return builder.ToString();
    }
}