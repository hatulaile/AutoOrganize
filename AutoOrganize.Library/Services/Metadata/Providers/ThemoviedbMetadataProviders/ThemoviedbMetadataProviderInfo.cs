using System.Diagnostics.CodeAnalysis;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Models.Metadata;

namespace AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbMetadataProviders;

public sealed class ThemoviedbMetadataProviderInfo : IUrlMetadataProviderInfo
{
    public string ProviderId => nameof(MetadataProviderType.ThemovieDB);
    public string HomeUrl => "https://www.themoviedb.org/";

    public bool TryGetUrl(string id, MetadataType metadataType, [NotNullWhen(true)] out string? uri)
    {
        uri = metadataType switch
        {
            MetadataType.Tv => GetTvUrl(id),
            MetadataType.Movie => GetMovieUrl(id),
            _ => null
        };

        return !string.IsNullOrEmpty(uri);
    }

    public string GetTvUrl(string id)
    {
        return $"https://www.themoviedb.org/tv/{id}";
    }

    public string GetMovieUrl(string id)
    {
        return $"https://www.themoviedb.org/movie/{id}";
    }
}