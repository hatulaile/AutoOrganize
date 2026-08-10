using System.Globalization;
using System.Runtime.CompilerServices;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.Metadata.Models.Metadata;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Tv;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Tv;
using AutoOrganize.Library.Services.RequestCoalescers;
using Microsoft.Extensions.Caching.Memory;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;
using TMDbLib.Objects.TvShows;

namespace AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbProviders;

public sealed partial class ThemoviedbProvider
{
    public async Task<IEnumerable<SeriesSearchResult>?> SearchAsync
        (SeriesSearchRequest request, bool ignoreCache = false, CancellationToken token = default)
    {
        IEnumerable<SeriesSearchResult>? results =
            await SearchTvRawForIdAsync(request, ignoreCache, token).ConfigureAwait(false);
        if (results is not null) return results;
        return (await SearchTvRawForNameAsync(request, ignoreCache, token).ConfigureAwait(false)).Select(SearchTvToResult);
    }

    public async Task<SeriesMetadata?> GetMetadataAsync(SeriesMetadataRequest request,
        bool ignoreCache = false, CancellationToken token = default)
    {
        (TvShow tvShow, ImagesWithId? images)? raw =
            await GetTvSeriesRawAsync(request, ignoreCache, token).ConfigureAwait(false);
        return !raw.HasValue ? null : TvShowToMetadata(raw.Value.tvShow, raw.Value.images);
    }

    public async Task<SeasonMetadata?> GetMetadataAsync(SeasonMetadataRequest request,
        bool ignoreCache = false, CancellationToken token = default)
    {
        int? tvId = await GetSeriesIdAsync(request.SeriesName, request.SeriesProviderIds, request.Language, null,
            token).ConfigureAwait(false);
        if (tvId is null) return null;

        string cacheKey = $"metadata_season_{tvId}_{request.SeasonNumber}_{request.Language}_{request.ImageLanguages}";

        ILease? lease;
        do
        {
            if (!ignoreCache && _cache.TryGetValue(cacheKey, out SeasonMetadata? cached) && cached is not null)
                return cached;

            (bool acquired, lease) =
                await _flightCoordinator.AcquireAsync(cacheKey, token).ConfigureAwait(false);

            if (acquired)
                break;
        } while (true);

        try
        {
            TvSeason? season = await Client.GetTvSeasonAsync(tvId.Value, request.SeasonNumber,
                    TvSeasonMethods.ExternalIds, language: request.Language, cancellationToken: token)
                .ConfigureAwait(false);

            if (season is null) return null;

            PosterImages? images = await Client.GetTvSeasonImagesAsync(tvId.Value, request.SeasonNumber,
                request.ImageLanguages, null, token).ConfigureAwait(false);
            var metadata = TvSeasonToMetadata(season, images);
            _cache.Set(cacheKey, metadata, CacheTime);
            return metadata;
        }
        finally
        {
            lease?.Dispose();
        }
    }

    public async Task<EpisodeMetadata?> GetMetadataAsync(EpisodeMetadataRequest request,
        bool ignoreCache = false, CancellationToken token = default)
    {
        int? tvId = await GetSeriesIdAsync(request.SeriesName, request.SeriesProviderIds, request.Language,
            request.Year, token).ConfigureAwait(false);
        if (tvId is null) return null;

        string cacheKey =
            $"metadata_episode_{tvId}_{request.SeasonNumber}_{request.EpisodeNumber}_{request.Language}_{request.ImageLanguages}";

        ILease? lease;
        do
        {
            if (!ignoreCache && _cache.TryGetValue(cacheKey, out EpisodeMetadata? cached) && cached is not null)
                return cached;

            (bool acquired, lease) =
                await _flightCoordinator.AcquireAsync(cacheKey, token).ConfigureAwait(false);

            if (acquired)
                break;
        } while (true);

        try
        {
            TvEpisode? episode = await Client.GetTvEpisodeAsync(tvId.Value, request.SeasonNumber, request.EpisodeNumber,
                    TvEpisodeMethods.ExternalIds, language: request.Language, cancellationToken: token)
                .ConfigureAwait(false);

            if (episode is null) return null;

            StillImages? images = await Client.GetTvEpisodeImagesAsync(tvId.Value, request.SeasonNumber,
                (int)request.EpisodeNumber, request.ImageLanguages, null, token).ConfigureAwait(false);
            var metadata = TvEpisodeToMetadata(episode, images);
            _cache.Set(cacheKey, metadata, CacheTime);
            return metadata;
        }
        finally
        {
            lease?.Dispose();
        }
    }

    private async Task<(TvShow, ImagesWithId?)?> GetTvSeriesRawAsync(SeriesMetadataRequest request,
        bool ignoreCache = false,
        CancellationToken token = default)
    {
        int? tvId = await GetSeriesIdAsync(request.Name, request.ProviderIds, request.Language, null, token)
            .ConfigureAwait(false);

        if (tvId is null) return null;

        string cacheKey = $"metadata_series_{tvId}_{request.Language}_{request.ImageLanguages}";

        ILease? lease;
        do
        {
            if (!ignoreCache && _cache.TryGetValue(cacheKey, out (TvShow, ImagesWithId)? cached) && cached is not null)
                return cached;

            (bool acquired, lease) =
                await _flightCoordinator.AcquireAsync(cacheKey, token).ConfigureAwait(false);

            if (acquired)
                break;
        } while (true);

        try
        {
            TvShow? tvShow = await Client.GetTvShowAsync(tvId.Value, extraMethods: TvShowMethods.ExternalIds,
                language: request.Language, cancellationToken: token).ConfigureAwait(false);
            if (tvShow is null) return null;

            ImagesWithId? images = await Client.GetTvShowImagesAsync(tvId.Value, request.ImageLanguages, null, token)
                .ConfigureAwait(false);
            (TvShow, ImagesWithId?) metadata = (tvShow, images);
            _cache.Set(cacheKey, metadata, CacheTime);
            return metadata;
        }
        finally
        {
            lease?.Dispose();
        }
    }

    private async Task<IEnumerable<SearchTv>> SearchTvRawForNameAsync(SeriesSearchRequest request, bool ignoreCache,
        CancellationToken token)
    {
        if (request.Name is null) return [];
        string cacheKey = $"search_tv_{request.Name}_{request.FirstAirDateYear}_{request.Language}";
        ILease? lease;
        do
        {
            if (!ignoreCache && _cache.TryGetValue(cacheKey, out IEnumerable<SearchTv>? cached) &&
                cached is not null)
                return cached;

            (bool acquired, lease) =
                await _flightCoordinator.AcquireAsync(cacheKey, token).ConfigureAwait(false);

            if (acquired)
                break;
        } while (true);

        try
        {
            await IfNotHasConfigGet(token).ConfigureAwait(false);

            SearchContainer<SearchTv>? container = await Client.SearchTvShowAsync(request.Name, request.Language,
                firstAirDateYear: request.FirstAirDateYear ?? 0, cancellationToken: token).ConfigureAwait(false);
            List<SearchTv>? searchTvs = container?.Results;

            if (searchTvs is { Count: > 0 })
                _cache.Set(cacheKey, searchTvs, CacheTime);

            return searchTvs ?? [];
        }
        finally
        {
            lease?.Dispose();
        }
    }

    private async Task<IEnumerable<SeriesSearchResult>?> SearchTvRawForIdAsync
        (SeriesSearchRequest request, bool ignoreCache, CancellationToken token)
    {
        if (request.ProviderIds?.TryGetValue(nameof(ProviderType.ThemovieDB), out var id) is not true ||
            !int.TryParse(id, out _))
            return null;

        (TvShow tvShow, ImagesWithId? images)? series = await GetTvSeriesRawAsync(new SeriesMetadataRequest
        {
            ProviderIds = request.ProviderIds,
            Language = request.Language
        }, ignoreCache, token).ConfigureAwait(false);
        if (!series.HasValue)
            return null;

        return [TvShowToSearchResult(series.Value.Item1)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async Task<int?> GetSeriesIdAsync(string? name, IProviderIds? providerIds, string? language, int? year,
        CancellationToken token)
    {
        await IfNotHasConfigGet(token).ConfigureAwait(false);

        if (providerIds?.TryGetValue(nameof(ProviderType.ThemovieDB), out string? id) is true &&
            int.TryParse(id, out int tvId))
            return tvId;

        SearchTv? first = (await SearchTvRawForNameAsync(new SeriesSearchRequest
        {
            Name = name ?? string.Empty,
            Language = language,
            FirstAirDateYear = year
        }, false, token).ConfigureAwait(false)).FirstOrDefault();

        return first?.Id;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SeriesSearchResult SearchTvToResult(SearchTv searchTv) => new()
    {
        Name = searchTv.Name,
        OriginalName = searchTv.OriginalName,
        OriginCountry = searchTv.OriginCountry?.Select(static x => new RegionInfo(x)).ToList(),
        FirstAirDate = searchTv.FirstAirDate,
        ProviderIds = new ProviderIds { [nameof(ProviderType.ThemovieDB)] = searchTv.Id.ToString() }
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SeriesSearchResult TvShowToSearchResult(TvShow tvShow) => new()
    {
        Name = tvShow.Name,
        OriginalName = tvShow.OriginalName,
        OriginCountry = tvShow.OriginCountry?.Select(static x => new RegionInfo(x)).ToList(),
        FirstAirDate = tvShow.FirstAirDate,
        ProviderIds = new ProviderIds { [nameof(ProviderType.ThemovieDB)] = tvShow.Id.ToString() }
    };

    //todo：如果有新的Provider，需要把额外id加入
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SeriesMetadata TvShowToMetadata(TvShow tvShow, ImagesWithId? images) => new()
    {
        Name = tvShow.Name,
        Overview = tvShow.Overview,
        AirDate = tvShow.FirstAirDate,
        OriginalName = tvShow.OriginalName,
        InProduction = tvShow.InProduction,
        Backdrops = ImageDataListToGroup(images?.Backdrops),
        Posters = ImageDataListToGroup(images?.Posters),
        Logos = ImageDataListToGroup(images?.Logos),
        Languages = tvShow.Languages?.Select(static x => new CultureInfo(x)).ToList(),
        Countries = tvShow.OriginCountry?.Select(static x => new RegionInfo(x)).ToList(),
        ProviderIds = new ProviderIds { [nameof(ProviderType.ThemovieDB)] = tvShow.Id.ToString() }
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SeasonMetadata TvSeasonToMetadata(TvSeason season, PosterImages? images)
    {
        var metadata = new SeasonMetadata
        {
            Name = season.Name,
            Overview = season.Overview,
            AirDate = season.AirDate,
            SeasonNumber = season.SeasonNumber,
            Posters = ImageDataListToGroup(images?.Posters)
        };

        if (season.Id is not null)
            metadata.ProviderIds.TryAdd(nameof(ProviderType.ThemovieDB), season.Id.Value.ToString());

        return metadata;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private EpisodeMetadata TvEpisodeToMetadata(TvEpisode episode, StillImages? images)
    {
        var metadata = new EpisodeMetadata
        {
            Name = episode.Name,
            Overview = episode.Overview,
            AirDate = episode.AirDate,
            EpisodeNumber = episode.EpisodeNumber,
            Backdrops = ImageDataListToGroup(images?.Stills)
        };

        if (episode.Id is not null)
            metadata.ProviderIds.TryAdd(nameof(ProviderType.ThemovieDB), episode.Id.Value.ToString());

        return metadata;
    }
}