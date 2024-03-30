using System.Collections.Generic;
using System.IO;
using System.Linq;
using LethalCompanyInputUtils.Config;
using LethalCompanyInputUtils.Utils;

namespace LethalCompanyInputUtils.Localization;

internal static class LocaleManager
{
    private static readonly Dictionary<string, string> LocaleEntries = new();

    internal static void LoadLocaleData()
    {
        var localeOverlay = new Stack<Locale>();
        
        var localeKey = InputUtilsConfig.localeKey.Value;
        var localePath = Path.Combine(FsUtils.LocaleDir, $"{localeKey}.json");
        
        if (!File.Exists(localePath))
        {
            Logging.Warn($"Could not find Locale {localeKey} at `{localePath}`!\nFalling back to `en_US`");
            
            localeKey = "en_US";
            localePath = Path.Combine(FsUtils.LocaleDir, $"{localeKey}.json");
        }
        
        do
        {
            if (!File.Exists(localePath))
                break;
            
            var locale = Locale.LoadFrom(localePath);
            
            localeOverlay.Push(locale);

            localeKey = locale.fallback;
            localePath = Path.Combine(FsUtils.LocaleDir, $"{localeKey}.json");
        } while (localeKey is not null);

        while (localeOverlay.TryPop(out var locale))
        {
            foreach (var (token, text) in locale.entries)
            {
                LocaleEntries[token] = text;
            }
        }
    }

    internal static string[] GetAllLocales() =>
        Directory.GetFiles(FsUtils.LocaleDir, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .ToArray();

    public static string GetString(string token) => LocaleEntries[token];
}