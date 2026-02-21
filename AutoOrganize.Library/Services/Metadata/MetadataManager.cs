using System.Diagnostics.CodeAnalysis;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Models.Metadata;
using AutoOrganize.Library.Models.Metadata.Movie;
using AutoOrganize.Library.Models.Metadata.Tv;
using AutoOrganize.Library.Services.Caches;
using AutoOrganize.Library.Services.Metadata.Providers;
using AutoOrganize.Library.Services.RequestCoalescers;
using AutoOrganize.Library.Utils;

namespace AutoOrganize.Library.Services.Metadata;

public sealed class MetadataManager : IMetadataManager
{
    //todo: logger
    //todo: exception handling
    //todo: default language setting
    public const string LANGUAGE_DEFAULT = "zh-CN";

    private IEnumerable<IMetadataProvider> Providers => field.OrderByDescending(p => p.Config.Priority);
    private readonly IFlightCoordinator _flightCoordinator;
    private readonly IMetadataCache _metadataCache;

    public async Task<SeriesMetadata?> SearchSeriesSingleAsync(SearchQuery query,
        CancellationToken token = default)
    {
        string cacheKey = CacheKeyUtils.GetSeries(query.Name);
        var cacheAcquireResult = await GetCacheOrWaitAsync<SeriesMetadata>(cacheKey, token).ConfigureAwait(false);
        if (cacheAcquireResult.HasMetadata)
            return cacheAcquireResult.Metadata;
        using IFlightLease acquireResultLease = cacheAcquireResult.FlightLease;

        SeriesMetadata? series = null;
        foreach (var provider in Providers)
        {
            if (provider is not ITvMetadataProvider tvMetadataProvider) continue;
            var temp = await tvMetadataProvider.SearchSeriesSingleAsync(query, LANGUAGE_DEFAULT,
                token).ConfigureAwait(false);
            if (temp is null) continue;

            if (temp.OriginalName is not null &&
                _metadataCache.TryGet(CacheKeyUtils.GetSeries(temp.OriginalName), out series))
                return series;


            if (series is null) series = temp;
            else series.Complement(temp);
        }

        if (series?.IsComplete() is not true)
            return null;

        _metadataCache.Set(CacheKeyUtils.GetSeries(series.OriginalName), series);
        _metadataCache.Set(cacheKey, series);
        return series;
    }

    public async Task<SeasonMetadata?> SearchSeasonAsync(SearchQuery query, int seasonNumber,
        CancellationToken token = default)
    {
        string cacheKey = CacheKeyUtils.GetSeason(query.Name, seasonNumber);
        var cacheAcquireResult = await GetCacheOrWaitAsync<SeasonMetadata>(cacheKey, token).ConfigureAwait(false);
        if (cacheAcquireResult.HasMetadata)
            return cacheAcquireResult.Metadata;
        using IFlightLease acquireResultLease = cacheAcquireResult.FlightLease;

        SeriesMetadata? series = await SearchSeriesSingleAsync(query, token).ConfigureAwait(false);
        if (series?.OriginalName is null)
            return null;

        if (_metadataCache.TryGet(CacheKeyUtils.GetSeason(series.OriginalName, seasonNumber),
                out SeasonMetadata? season))
        {
            _metadataCache.Set(cacheKey, season);
            return season;
        }

        foreach (var provider in Providers)
        {
            if (provider is not IMetadataProvider<IMetadataProviderInfo> metadataProvider ||
                series.ExternalIds is null ||
                !series.ExternalIds.TryGetValue(metadataProvider.Info.ProviderId, out string? id) ||
                provider is not ITvMetadataProvider tvMetadataProvider)
                continue;

            var temp = await tvMetadataProvider.GetSeasonMetadataAsync(id, seasonNumber, LANGUAGE_DEFAULT, token)
                .ConfigureAwait(false);
            if (temp is null) continue;

            if (season is null) season = temp;
            else season.Complement(temp);
        }

        if (season?.IsComplete() is not true)
            return null;

        series.AddChild(season);
        _metadataCache.Set(CacheKeyUtils.GetSeason(series.OriginalName, seasonNumber), season);
        _metadataCache.Set(CacheKeyUtils.GetSeason(query.Name, seasonNumber), season);
        return season;
    }

    public async Task<EpisodeMetadata?> SearchEpisodeAsync(SearchQuery query, int seasonNumber, long episodeNumber,
        CancellationToken token = default)
    {
        string cacheKey = CacheKeyUtils.GetEpisode(query.Name, seasonNumber, episodeNumber);
        var cacheAcquireResult = await GetCacheOrWaitAsync<EpisodeMetadata>(cacheKey, token).ConfigureAwait(false);
        if (cacheAcquireResult.HasMetadata)
            return cacheAcquireResult.Metadata;
        using IFlightLease acquireResultLease = cacheAcquireResult.FlightLease;

        SeasonMetadata? season = await SearchSeasonAsync(query, seasonNumber, token).ConfigureAwait(false);
        if (season?.Series?.OriginalName is null)
            return null;

        if (_metadataCache.TryGet(
                CacheKeyUtils.GetEpisode(season.Series.OriginalName, seasonNumber, episodeNumber),
                out EpisodeMetadata? episode))
        {
            _metadataCache.Set(cacheKey, episode);
            return episode;
        }

        foreach (var provider in Providers)
        {
            if (provider is not IMetadataProvider<IMetadataProviderInfo> metadataProvider ||
                season.Series.ExternalIds is null ||
                !season.Series.ExternalIds.TryGetValue(metadataProvider.Info.ProviderId, out string? id) ||
                provider is not ITvMetadataProvider tvMetadataProvider)
                continue;

            var temp = await tvMetadataProvider.GetEpisodeMetadataAsync(id,
                    season.SeasonNumber ?? throw new NullReferenceException(), episodeNumber, LANGUAGE_DEFAULT,
                    token)
                .ConfigureAwait(false);
            if (temp is null) continue;

            if (episode is null) episode = temp;
            else episode.Complement(temp);
        }

        if (episode?.IsComplete() is not true)
            return null;

        season.AddChild(episode);
        _metadataCache.Set(CacheKeyUtils.GetEpisode(season.Series.OriginalName, seasonNumber, episodeNumber),
            episode);
        _metadataCache.Set(cacheKey, episode);
        return episode;
    }

    public async Task<MovieMetadata?> SearchMovieSingleAsync(SearchQuery query,
        CancellationToken token = default)
    {
        string cacheKey = CacheKeyUtils.GetMovie(query.Name);
        var cacheAcquireResult = await GetCacheOrWaitAsync<MovieMetadata>(cacheKey, token).ConfigureAwait(false);
        if (cacheAcquireResult.HasMetadata)
            return cacheAcquireResult.Metadata;
        using IFlightLease acquireResultLease = cacheAcquireResult.FlightLease;

        MovieMetadata? movie = null;
        foreach (var provider in Providers)
        {
            if (provider is not IMovieMetadataProvider movieMetadataProvider) continue;
            var temp = await movieMetadataProvider.SearchMovieSingleAsync(query, LANGUAGE_DEFAULT, token)
                .ConfigureAwait(false);
            if (temp is null) continue;

            if (movie is null) movie = temp;
            else movie.Complement(temp);
        }

        if (movie?.IsComplete() is not true)
            return null;

        _metadataCache.Set(CacheKeyUtils.GetMovie(movie.OriginalName), movie);
        _metadataCache.Set(cacheKey, movie);
        return movie;
    }

    private async Task<CacheAcquireResult<TMetadata>> GetCacheOrWaitAsync<TMetadata>(
        string cacheKey, CancellationToken token = default) where TMetadata : MetadataBase
    {
        if (_metadataCache.TryGet(cacheKey, out TMetadata? metadata))
            return new CacheAcquireResult<TMetadata>(metadata);
        FlightCoordinatorAcquireResult acquireResult;
        do
        {
            acquireResult = await _flightCoordinator.AcquireAsync(cacheKey, token: token).ConfigureAwait(false);
            if (!acquireResult.Acquired && _metadataCache.TryGet(cacheKey, out metadata))
                return new CacheAcquireResult<TMetadata>(metadata);
        } while (!acquireResult.Acquired);

        return new CacheAcquireResult<TMetadata>(acquireResult.Lease);
    }

    public MetadataManager(IEnumerable<IMetadataProvider> providers, IFlightCoordinator flightCoordinator,
        IMetadataCache metadataCache)
    {
        Providers = providers;
        _flightCoordinator = flightCoordinator;
        _metadataCache = metadataCache;
    }

    private sealed class CacheAcquireResult<TMetadata> where TMetadata : MetadataBase
    {
        public TMetadata? Metadata { get; set; }

        public IFlightLease? FlightLease { get; set; }

        [MemberNotNullWhen(true, nameof(Metadata))]
        [MemberNotNullWhen(false, nameof(FlightLease))]
        public bool HasMetadata => Metadata is not null;

        [MemberNotNullWhen(false, nameof(Metadata))]
        [MemberNotNullWhen(true, nameof(FlightLease))]
        public bool HasFlightLease => FlightLease is not null;


        public CacheAcquireResult(IFlightLease flightLease)
        {
            FlightLease = flightLease;
        }

        public CacheAcquireResult(TMetadata metadata)
        {
            Metadata = metadata;
        }

        public void Deconstruct(out IFlightLease? flightLease, out TMetadata? metadata)
        {
            flightLease = FlightLease;
            metadata = Metadata;
        }
    }
}