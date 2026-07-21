using System.Diagnostics.CodeAnalysis;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;
using AutoOrganize.Library.Services.Metadata.Providers.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbProviders;

public sealed class ThemoviedbProviderInfo : IProviderInfo, IUriProviderInfo
{
    public string ProviderName => "TMDB";

    public string ProviderId => nameof(ProviderType.ThemovieDB);

    public Uri? HomeUri { get; } = new("https://www.themoviedb.org/");

    public bool TryGetUri(string id, MetadataType type, [NotNullWhen(true)] out Uri? uri)
    {
        switch (type)
        {
            case MetadataType.None:
                uri = null;
                return false;
            case MetadataType.Tv:
                uri  = new Uri($"https://www.themoviedb.org/tv/{id}", UriKind.Absolute);
                return true;
            case MetadataType.Movie:
                uri = new Uri($"https://www.themoviedb.org/movie/{id}", UriKind.Absolute);
                return true;
            default:
                uri = null;
                return false;
        }
    }
}