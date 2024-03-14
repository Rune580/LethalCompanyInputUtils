using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace LethalCompanyInputUtils.Localization;

[Serializable]
internal class Locale
{
    public string? fallback;

    public Dictionary<string, string> entries = new();

    public static Locale LoadFrom(string jsonPath)
    {
        var contents = File.ReadAllText(jsonPath);
        
        var locale = JsonConvert.DeserializeObject<Locale>(contents);
        if (locale is null)
            throw new NullReferenceException();

        return locale;
    }
}