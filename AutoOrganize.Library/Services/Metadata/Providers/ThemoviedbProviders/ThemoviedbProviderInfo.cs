using System.Diagnostics.CodeAnalysis;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Movie;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;
using AutoOrganize.Library.Services.Metadata.Providers.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbProviders;

public sealed class ThemoviedbProviderInfo : IProviderInfo, IUriProviderInfo
{
    public string ProviderName => "TMDB";

    public string ProviderId => nameof(ProviderType.ThemovieDB);

    public Uri? HomeUri { get; } = new("https://www.themoviedb.org/");

    public bool TryGetUri(MetadataBase metadata, [NotNullWhen(true)] out Uri? uri)
    {
        string? id;
        switch (metadata)
        {
            case SeriesMetadata series:
                if (!series.ProviderIds.TryGetValue(ProviderId, out id))
                    goto default;
                uri = new Uri($"https://www.themoviedb.org/tv/{id}", UriKind.Absolute);
                return true;
            case SeasonMetadata season:
                if (season.Series?.ProviderIds.TryGetValue(ProviderId, out var seriesId) is not true)
                    goto default;
                uri = new Uri($"https://www.themoviedb.org/tv/{seriesId}/season/{season.SeasonNumber}",
                    UriKind.Absolute);
                return true;
            case EpisodeMetadata episode:
                if (episode.Series?.ProviderIds.TryGetValue(ProviderId, out seriesId) is not true)
                    goto default;
                uri = new Uri($"https://www.themoviedb.org/tv/{seriesId}/season/{episode.SeasonNumber}/episode/{episode.EpisodeNumber}",
                    UriKind.Absolute);
                return true;
            case MovieMetadata movie:
                if (!movie.ProviderIds.TryGetValue(ProviderId, out id))
                    goto default;
                uri = new Uri($"https://www.themoviedb.org/movie/{id}", UriKind.Absolute);
                return true;
            default:
                uri = null;
                return false;
        }
    }
}