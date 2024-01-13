using System.Collections.Generic;
using System.Text;
using UnityEngine;

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

    public static void DrawGizmoUiRect(this RectTransform rectTransform)
    {
        var z = rectTransform.position.z;
        var rect = rectTransform.UiBounds();
        
        var bl = new Vector3(rect.min.x, rect.min.y, z);
        var tl = new Vector3(rect.min.x, rect.max.y, z);
        var tr = new Vector3(rect.max.x, rect.max.y, z);
        var br = new Vector3(rect.max.x, rect.min.y, z);
        
        Gizmos.DrawLine(bl, tl);
        Gizmos.DrawLine(tl, tr);
        Gizmos.DrawLine(tr, br);
        Gizmos.DrawLine(br, bl);
    }
}