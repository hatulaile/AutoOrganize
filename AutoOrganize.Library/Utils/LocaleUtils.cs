using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace AutoOrganize.Library.Utils;

public static class LocaleUtils
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static CultureInfo? GetCultureInfo(string? iso_639_1, string? iso_3116_1)
    {
        if (iso_639_1 is null || iso_3116_1 is null)
            return null;

        try
        {
            return new CultureInfo($"{iso_639_1}-{iso_3116_1}");
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static string? GetLocale(string? iso_639_1, string? iso_3116_1)
    {
        if (iso_639_1 is null || iso_3116_1 is null)
            return null;

        return $"{iso_639_1}-{iso_3116_1}";
    }
}