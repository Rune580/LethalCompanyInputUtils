namespace LethalCompanyInputUtils.SourceGen;

internal static class StringUtils
{
    public static string ToCamelCase(this string text)
    {
        var textLowerCase = text.ToLower();

        var textCamelCase = "";
        for (int i = 0; i < text.Length; i++)
        {
            if (i == 0)
            {
                textCamelCase += textLowerCase[i];
                continue;
            }

            textCamelCase += text[i];
        }

        return textCamelCase;
    }

    public static string ToPascalCase(this string text)
    {
        if (int.TryParse(text, out var num))
            return $"Num{num}";
        
        var textUpperCase = text.ToUpper();
        var capitalizeNext = false;
            
        var textPascalCase = "";
        for (int i = 0; i < text.Length; i++)
        {
            if (i == 0)
            {
                textPascalCase += textUpperCase[i];
                continue;
            }

            if (text[i] == ' ' || text[i] == '/')
            {
                capitalizeNext = true;
                continue;
            }

            if (capitalizeNext)
            {
                capitalizeNext = false;
                textPascalCase += textUpperCase[i];
            }
            else
            {
                textPascalCase += text[i];
            }
        }

        return textPascalCase;
    }
}