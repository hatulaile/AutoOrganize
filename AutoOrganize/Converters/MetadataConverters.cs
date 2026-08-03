using AutoOrganize.Library.Models;
using Avalonia.Data.Converters;

namespace AutoOrganize.Converters;

public static class MetadataConverters
{
    public static FuncValueConverter<string, string> MetadataProvidersIConConverter { get; } = new(providerName =>
    {
        if (providerName is null)
            return string.Empty;

        return providerName switch
        {
            nameof(ProviderType.ThemovieDB) => "/Assets/Images/TheMovieDB/PrimaryFull.svg",
            _ => string.Empty
        };
    });
}