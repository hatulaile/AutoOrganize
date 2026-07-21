using System.Runtime.CompilerServices;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.Metadata.Models.Metadata;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Tv;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Tv;
using Microsoft.Extensions.Caching.Memory;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;
using TMDbLib.Objects.TvShows;

namespace AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbProviders;

public sealed partial class ThemoviedbProvider
{
    public async Task<IEnumerable<SeriesSearchResult>?> SearchAsync
        (SeriesSearchRequest request, bool ignoreCache = false, CancellationToken token = default) =>
        (await SearchTvRawAsync(request, ignoreCache, token).ConfigureAwait(false)).Select(SearchTvToResult);

    public async Task<SeriesMetadata?> GetMetadataAsync(SeriesMetadataRequest request,
        bool ignoreCache = false, CancellationToken token = default)
    {
        int? tvId = await GetSeriesIdAsync(request.Name, request.ProviderIds, request.Language, token)
            .ConfigureAwait(false);

        if (tvId is null) return null;

        string cacheKey = $"metadata_series_{tvId}_{request.Language}_{request.ImageLanguages}";
        if (!ignoreCache && _cache.TryGetValue(cacheKey, out SeriesMetadata? cached) && cached is not null)
            return cached;

        var methods = TvShowMethods.Images | TvShowMethods.ExternalIds;
        TvShow? tvShow = await Client.GetTvShowAsync(tvId.Value, extraMethods: methods,
            language: request.Language, includeImageLanguage: request.ImageLanguages,
            cancellationToken: token).ConfigureAwait(false);
        if (tvShow is null) return null;

        var metadata = TvShowToMetadata(tvShow);
        _cache.Set(cacheKey, metadata, CacheTime);
        return metadata;
    }

    public async Task<SeasonMetadata?> GetMetadataAsync(SeasonMetadataRequest request,
        bool ignoreCache = false, CancellationToken token = default)
    {
        int? tvId = await GetSeriesIdAsync(request.Name, request.ProviderIds, request.Language, token)
            .ConfigureAwait(false);
        if (tvId is null) return null;

        string cacheKey = $"metadata_season_{tvId}_{request.SeasonNumber}_{request.Language}_{request.ImageLanguages}";
        if (!ignoreCache && _cache.TryGetValue(cacheKey, out SeasonMetadata? cached) && cached is not null)
            return cached;

        var methods = TvSeasonMethods.Images | TvSeasonMethods.ExternalIds;
        TvSeason? season = await Client.GetTvSeasonAsync(tvId.Value, request.SeasonNumber,methods,
            language: request.Language, includeImageLanguage: request.ImageLanguages,
            cancellationToken: token).ConfigureAwait(false);

        if (season is null) return null;

        var metadata = TvSeasonToMetadata(season);
        _cache.Set(cacheKey, metadata, CacheTime);
        return metadata;
    }

    public async Task<EpisodeMetadata?> GetMetadataAsync(EpisodeMetadataRequest request,
        bool ignoreCache = false, CancellationToken token = default)
    {
        int? tvId = await GetSeriesIdAsync(request.Name, request.ProviderIds, request.Language, token)
            .ConfigureAwait(false);
        if (tvId is null) return null;

        string cacheKey = $"metadata_episode_{tvId}_{request.SeasonNumber}_{request.EpisodeNumber}_{request.Language}_{request.ImageLanguages}";
        if (!ignoreCache && _cache.TryGetValue(cacheKey, out EpisodeMetadata? cached) && cached is not null)
            return cached;

        var methods = TvEpisodeMethods.Images | TvEpisodeMethods.ExternalIds;
        TvEpisode? episode = await Client.GetTvEpisodeAsync(tvId.Value, request.SeasonNumber, request.EpisodeNumber,methods,
            language: request.Language, includeImageLanguage: request.ImageLanguages,
            cancellationToken: token).ConfigureAwait(false);

        if (episode is null) return null;

        var metadata = TvEpisodeToMetadata(episode);
        _cache.Set(cacheKey, metadata, CacheTime);
        return metadata;
    }

    private async Task<IEnumerable<SearchTv>> SearchTvRawAsync(SeriesSearchRequest request, bool ignoreCache,
        CancellationToken token)
    {
        string cacheKey = GetSearchCacheKey(request.Name, request.FirstAirDateYear, request.Language);
        if (!ignoreCache && _cache.TryGetValue(cacheKey, out IEnumerable<SearchTv>? cached) && cached is not null)
            return cached;

        await IfNotHasConfigGet(token).ConfigureAwait(false);
        SearchContainer<SearchTv>? container = await Client.SearchTvShowAsync(request.Name, request.Language,
            firstAirDateYear: request.FirstAirDateYear ?? 0, cancellationToken: token).ConfigureAwait(false);

        var searchTvs = container?.Results;
        if (searchTvs is { Count: > 0 })
            _cache.Set(cacheKey, searchTvs, CacheTime);

        return searchTvs ?? [];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async Task<int?> GetSeriesIdAsync(string? name, ProviderIds? providerIds, string? language,
        CancellationToken token)
    {
        if (providerIds?.TryGetValue(nameof(ProviderType.ThemovieDB), out string? id) == true &&
            int.TryParse(id, out int tvId))
            return tvId;

        await IfNotHasConfigGet(token).ConfigureAwait(false);

        SearchTv? first = (await SearchTvRawAsync(new SeriesSearchRequest
        {
            Name = name ?? string.Empty,
            Language = language
        }, false, token).ConfigureAwait(false)).FirstOrDefault();

        return first?.Id;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SeriesSearchResult SearchTvToResult(SearchTv searchTv) => new()
    {
        Name = searchTv.Name,
        OriginalName = searchTv.OriginalName,
        OriginCountry = searchTv.OriginCountry is { Count: > 0 } ? string.Join(", ", searchTv.OriginCountry) : null,
        FirstAirDate = searchTv.FirstAirDate,
        ProviderIds = { [nameof(ProviderType.ThemovieDB)] = searchTv.Id.ToString() }
    };

    //todo：如果有新的Provider，需要把额外id加入
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SeriesMetadata TvShowToMetadata(TvShow tvShow) => new()
    {
        Name = tvShow.Name,
        Overview = tvShow.Overview,
        AirDate = tvShow.FirstAirDate,
        OriginalName = tvShow.OriginalName,
        InProduction = tvShow.InProduction,
        Backdrops = ImageDataListToGroup(tvShow.Images?.Backdrops),
        Posters = ImageDataListToGroup(tvShow.Images?.Posters),
        Logos = ImageDataListToGroup(tvShow.Images?.Logos),
        ProviderIds = { [nameof(ProviderType.ThemovieDB)] = tvShow.Id.ToString() }
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SeasonMetadata TvSeasonToMetadata(TvSeason season)
    {
        var metadata = new SeasonMetadata
        {
            Name = season.Name,
            Overview = season.Overview,
            AirDate = season.AirDate,
            SeasonNumber = season.SeasonNumber,
            Posters = ImageDataListToGroup(season.PosterPath is not null
                ? [new ImageData { FilePath = season.PosterPath }]
                : null)
        };

        if (season.Id is not null)
            metadata.ProviderIds.TryAdd(nameof(ProviderType.ThemovieDB), season.Id.Value.ToString());

        return metadata;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private EpisodeMetadata TvEpisodeToMetadata(TvEpisode episode)
    {
        var metadata = new EpisodeMetadata
        {
            Name = episode.Name,
            Overview = episode.Overview,
            AirDate = episode.AirDate,
            EpisodeNumber = episode.EpisodeNumber,
            Backdrops = ImageDataListToGroup(episode.StillPath is not null
                ? [new ImageData { FilePath = episode.StillPath }]
                : null)
        };

        if (episode.Id is not null)
            metadata.ProviderIds.TryAdd(nameof(ProviderType.ThemovieDB), episode.Id.Value.ToString());

        return metadata;
    }
}