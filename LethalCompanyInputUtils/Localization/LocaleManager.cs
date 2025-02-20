using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LethalCompanyInputUtils.Components.Switch;
using LethalCompanyInputUtils.Config;
using LethalCompanyInputUtils.Utils;

namespace LethalCompanyInputUtils.Localization;

internal static class LocaleManager
{
    private static readonly Dictionary<string, Func<string>> Keywords = new();
    
    private static readonly Dictionary<string, string> LocaleEntries = new();

    internal static void LoadLocaleData()
    {
        if (Keywords.Count == 0)
            InitKeywords();
        
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
            
            if (localeKey != "en_US" && locale.fallback is null)
                localeKey = "en_US";
            else
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

    private static void InitKeywords()
    {
        Keywords.Add("CurrentContext", () => GetString($"Context.{BindingOverrideContextSwitch.currentContext}"));
        Keywords.Add("OppositeContext", () => GetString($"Context.{(BindingOverrideContextSwitch.currentContext is BindingOverrideType.Global ? BindingOverrideType.Local : BindingOverrideType.Global)}"));
        Keywords.Add("BindingOverridePriorityConfigValue", () =>
        {
            return InputUtilsConfig.bindingOverridePriority.Value switch
            {
                BindingOverridePriority.GlobalThenLocal => GetString("OverridePriority.PreferGlobal"),
                BindingOverridePriority.LocalThenGlobal => GetString("OverridePriority.PreferLocal"),
                BindingOverridePriority.GlobalOnly => GetString("OverridePriority.GlobalOnly"),
                BindingOverridePriority.LocalOnly => GetString("OverridePriority.LocalOnly"),
                _ => throw new ArgumentOutOfRangeException()
            };
        });
        Keywords.Add("OptionalPreferPriority", () =>
        {
            var currentContext = BindingOverrideContextSwitch.currentContext;
            var priority = InputUtilsConfig.bindingOverridePriority.Value;
            if (priority is BindingOverridePriority.GlobalOnly or BindingOverridePriority.LocalOnly
                || (priority is BindingOverridePriority.GlobalThenLocal && currentContext is BindingOverrideType.Local)
                || (priority is BindingOverridePriority.LocalThenGlobal && currentContext is BindingOverrideType.Global))
                return "";

            return GetString("RebindButton.DisabledByOverride.PopOver.Optional");
        });
    }

    internal static string[] GetAllLocales() =>
        Directory.GetFiles(FsUtils.LocaleDir, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .ToArray();

    public static bool ContainsToken(string token) => LocaleEntries.ContainsKey(token);

    public static string GetString(string token)
    {
        if (!LocaleEntries.TryGetValue(token, out var localizedString))
        {
            Logging.Error($"Could not find LocalizedString for \'{token}\'!");
            return token;
        }

        // Find keywords encapsulated with [], ensure they aren't escaped like [[]], replace matching keywords.
        foreach (var (keyword, func) in Keywords)
        {
            var index = localizedString.IndexOf(keyword, StringComparison.InvariantCulture);
            var startIndex = -1;

            while (index >= 0)
            {
                var prevIndex = index - 1;
                
                if (startIndex < 0)
                {
                    if (prevIndex < 0)
                    {
                        index = localizedString.IndexOf(keyword, index + 1, StringComparison.InvariantCulture);
                        continue;
                    }

                    if (localizedString[prevIndex] == '[' && (prevIndex - 1 < 0 || localizedString[prevIndex - 1] != '\\'))
                    {
                        startIndex = prevIndex;
                        continue;
                    }
                    
                    index = localizedString.IndexOf(keyword, index + 1, StringComparison.InvariantCulture);

                    continue;
                }

                var endIndex = localizedString.IndexOf(']', startIndex);
                if (endIndex < 0) // No matching end found, skip.
                    break;

                prevIndex = endIndex - 1;
                if (localizedString[prevIndex] == '\\')
                    break;

                try
                {
                    var replacement = func.Invoke();
                    
                    var keywordLength = (endIndex - startIndex) + 1;
                    var lengthDiff = replacement.Length - keywordLength;
                    
                    localizedString = localizedString.Remove(startIndex, keywordLength)
                        .Insert(startIndex, replacement);
                    
                    endIndex += lengthDiff;
                }
                catch (Exception e)
                {
                    Logging.Error($"Error caught during keyword replacement:\n{e}");
                }

                if (endIndex >= localizedString.Length)
                    break;
                
                // Try to the find next instance of keyword
                index = localizedString.IndexOf(keyword, endIndex, StringComparison.InvariantCulture);
                startIndex = -1;
            }
        }

        localizedString = localizedString.Replace("\\[", "[").Replace("\\]", "]");
        
        return localizedString;
    }
}