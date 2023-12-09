using System.Linq;
using System.Text;

namespace build.Utils;

internal static class StringExtensions
{
    public static string ToUpperSnakeCase(this string text)
    {
        StringBuilder builder = new StringBuilder();
        bool first = true;

        foreach (var character in text.Select(c => c.ToString()))
        {
            if (first)
            {
                first = false;
                builder.Append(character.ToUpper());
                continue;
            }

            if (character.IsUpperCase())
                builder.Append('_');

            builder.Append(character.ToUpper());
        }

        return builder.ToString();
    }

    private static bool IsUpperCase(this string text)
    {
        return string.Equals(text, text.ToUpper());
    }
}