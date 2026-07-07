using System.Globalization;
using System.Text;

namespace MisRecetas.Utils;

public static class TextNormalizer
{
    public static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";

        var normalized = input.Trim().ToLowerInvariant();
        var formD = normalized.Normalize(NormalizationForm.FormD);

        var sb = new StringBuilder();
        foreach (var c in formD)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}