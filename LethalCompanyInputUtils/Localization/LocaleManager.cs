using System.Collections.Generic;
using System.IO;
using LethalCompanyInputUtils.Utils;

namespace LethalCompanyInputUtils.Localization;

internal static class LocaleManager
{
    private static readonly Dictionary<string, string> LocaleEntries = new();

    internal static void Init()
    {
        var localeOverlay = new Stack<Locale>();
        var localeKey = "en_US";
        
        do
        {
            var localePath = Path.Combine(FsUtils.LocaleDir, $"{localeKey}.json");
            if (!File.Exists(localePath))
                break;
            
            var locale = Locale.LoadFrom(localePath);
            
            localeOverlay.Push(locale);

            localeKey = locale.fallback;
        } while (localeKey is not null);

        while (localeOverlay.TryPop(out var locale))
        {
            foreach (var (token, text) in locale.entries)
            {
                LocaleEntries[token] = text;
            }
        }
    }

    public static string GetString(string token) => LocaleEntries[token];
}