using System.Diagnostics.CodeAnalysis;
using AutoOrganize.Library.Models;
using AutoOrganize.Library.Models.Metadata;
using AutoOrganize.Library.Models.Metadata.Movie;
using AutoOrganize.Library.Models.Metadata.Tv;
using AutoOrganize.Library.Services.Caches;
using AutoOrganize.Library.Services.Metadata.Providers;
using AutoOrganize.Library.Services.RequestCoalescers;
using AutoOrganize.Library.Utils;
using Microsoft.Extensions.Logging;

namespace AutoOrganize.Library.Services.Metadata;

public sealed class MetadataManager : IMetadataManager
{
    //todo: exception handling
    //todo: default language setting
    public const string LANGUAGE_DEFAULT = "zh-CN";

    private IEnumerable<IMetadataProvider> AllProviders { get; }
    private readonly IFlightCoordinator _flightCoordinator;
    private readonly IMetadataCache _metadataCache;
    private readonly ILogger<MetadataManager> _logger;

    public async Task<SeriesMetadata?> SearchSeriesSingleAsync(SearchQuery query,
        CancellationToken token = default)
    {
        _logger.LogDebug("开始搜索剧集: {@Query}", query);

        string cacheKey = CacheKeyUtils.GetSeries(query.Name);
        var cacheAcquireResult = await GetCacheOrWaitAsync<SeriesMetadata>(cacheKey, token).ConfigureAwait(false);
        if (cacheAcquireResult.HasMetadata)
        {
            _logger.LogDebug("剧集缓存命中: {Name}", query.Name);
            return cacheAcquireResult.Metadata;
        }

        using IFlightLease acquireResultLease = cacheAcquireResult.FlightLease;

        SeriesMetadata? series = null;
        foreach (var provider in GetAvailableProviders())
        {
            if (provider is not ITvMetadataProvider tvMetadataProvider) continue;
            _logger.LogDebug("使用提供程序 {Provider} 搜索剧集: {@Query}", provider.Info.ProviderId, query);
            var temp = await tvMetadataProvider.SearchSeriesSingleAsync(query, LANGUAGE_DEFAULT,
                token).ConfigureAwait(false);
            if (temp is null)
            {
                _logger.LogDebug("提供程序 {Provider} 未找到剧集 {@Query}", provider.Info.ProviderId, query);
                continue;
            }

            if (temp.OriginalName is not null &&
                _metadataCache.TryGet(CacheKeyUtils.GetSeries(temp.OriginalName), out series))
            {
                _logger.LogDebug("Query {Name} 通过原始名称缓存找到剧集: {OriginalName}", query.Name, temp.OriginalName);
                _metadataCache.Set(cacheKey, series);
                return series;
            }

            if (series is null) series = temp;
            else series.Complement(temp);
        }

        if (series?.IsComplete() is not true)
        {
            _logger.LogWarning("未能找到完整的剧集元数据: {@Query}", query);
            return null;
        }

        _logger.LogInformation("成功搜索到剧集: {Name} ({Year})", series.Name, series.AirDate?.Year);
        _metadataCache.Set(CacheKeyUtils.GetSeries(series.OriginalName), series);
        _metadataCache.Set(cacheKey, series);
        return series;
    }

    public async Task<SeasonMetadata?> SearchSeasonAsync(SearchQuery query, int seasonNumber,
        CancellationToken token = default)
    {
        _logger.LogDebug("开始搜索季: {Query}, S{Season}", query, seasonNumber);

        string cacheKey = CacheKeyUtils.GetSeason(query.Name, seasonNumber);
        var cacheAcquireResult = await GetCacheOrWaitAsync<SeasonMetadata>(cacheKey, token).ConfigureAwait(false);
        if (cacheAcquireResult.HasMetadata)
        {
            _logger.LogDebug("季缓存命中: {Name} S{Season}", query.Name, seasonNumber);
            return cacheAcquireResult.Metadata;
        }

        using IFlightLease acquireResultLease = cacheAcquireResult.FlightLease;

        SeriesMetadata? series = await SearchSeriesSingleAsync(query, token).ConfigureAwait(false);
        if (series?.OriginalName is null)
        {
            _logger.LogWarning("未找到对应剧集，无法搜索季信息: {@Query}", query);
            return null;
        }

        if (_metadataCache.TryGet(CacheKeyUtils.GetSeason(series.OriginalName, seasonNumber),
                out SeasonMetadata? season))
        {
            _logger.LogDebug("通过原始名称缓存找到季: {OriginalName} S{Season}", series.OriginalName, seasonNumber);
            _metadataCache.Set(cacheKey, season);
            return season;
        }

        foreach (var provider in GetAvailableProviders())
        {
            if (provider is not IMetadataProvider<IMetadataProviderInfo> metadataProvider ||
                series.ExternalIds is null ||
                !series.ExternalIds.TryGetValue(metadataProvider.Info.ProviderId, out string? id) ||
                provider is not ITvMetadataProvider tvMetadataProvider)
                continue;

            _logger.LogDebug("使用提供程序 {Provider} 搜索季: 剧集ID={Id}, S{Season}",
                provider.Info.ProviderId, id, seasonNumber);

            var temp = await tvMetadataProvider.GetSeasonMetadataAsync(id, seasonNumber, LANGUAGE_DEFAULT, token)
                .ConfigureAwait(false);

            if (temp is null)
            {
                _logger.LogDebug("提供程序 {Provider} 未找到季信息", provider.Info.ProviderId);
                continue;
            }

            if (season is null) season = temp;
            else season.Complement(temp);
        }

        if (season?.IsComplete() is not true)
        {
            _logger.LogWarning("未能找到完整的季元数据: {@Query} S{Season}", query, seasonNumber);
            return null;
        }

        _logger.LogInformation("成功搜索到季: {SeriesName} S{Season}", series.Name, seasonNumber);
        series.AddChild(season);
        _metadataCache.Set(CacheKeyUtils.GetSeason(series.OriginalName, seasonNumber), season);
        _metadataCache.Set(CacheKeyUtils.GetSeason(query.Name, seasonNumber), season);
        return season;
    }

    public async Task<EpisodeMetadata?> SearchEpisodeAsync(SearchQuery query, int seasonNumber, long episodeNumber,
        CancellationToken token = default)
    {
        _logger.LogDebug("开始搜索集: {@Query}, S{Season}E{Episode}", query, seasonNumber, episodeNumber);
        string cacheKey = CacheKeyUtils.GetEpisode(query.Name, seasonNumber, episodeNumber);
        var cacheAcquireResult = await GetCacheOrWaitAsync<EpisodeMetadata>(cacheKey, token).ConfigureAwait(false);
        if (cacheAcquireResult.HasMetadata)
        {
            _logger.LogDebug("集缓存命中: {Name} S{Season}E{Episode}", query.Name, seasonNumber, episodeNumber);
            return cacheAcquireResult.Metadata;
        }

        using IFlightLease acquireResultLease = cacheAcquireResult.FlightLease;

        SeasonMetadata? season = await SearchSeasonAsync(query, seasonNumber, token).ConfigureAwait(false);
        if (season?.Series?.OriginalName is null)
        {
            _logger.LogWarning("未找到对应季，无法搜索集信息: {@Query} S{Season}", query, seasonNumber);
            return null;
        }

        if (_metadataCache.TryGet(
                CacheKeyUtils.GetEpisode(season.Series.OriginalName, seasonNumber, episodeNumber),
                out EpisodeMetadata? episode))
        {
            _logger.LogDebug("通过原始名称缓存找到集: {OriginalName} S{Season}E{Episode}",
                season.Series.OriginalName, seasonNumber, episodeNumber);
            _metadataCache.Set(cacheKey, episode);
            return episode;
        }

        foreach (var provider in GetAvailableProviders())
        {
            if (provider is not IMetadataProvider<IMetadataProviderInfo> metadataProvider ||
                season.Series.ExternalIds is null ||
                !season.Series.ExternalIds.TryGetValue(metadataProvider.Info.ProviderId, out string? id) ||
                provider is not ITvMetadataProvider tvMetadataProvider)
                continue;

            _logger.LogDebug("使用提供程序 {Provider} 搜索集: 剧集ID={Id}, S{Season}E{Episode}",
                provider.Info.ProviderId, id, seasonNumber, episodeNumber);

            var temp = await tvMetadataProvider.GetEpisodeMetadataAsync(id,
                    season.SeasonNumber ??
                    throw new InvalidOperationException("SeasonNumber is null when searching episode"), episodeNumber,
                    LANGUAGE_DEFAULT,
                    token)
                .ConfigureAwait(false);
            if (temp is null)
            {
                _logger.LogDebug("提供程序 {Provider} 未找到集信息", provider.Info.ProviderId);
                continue;
            }

            if (episode is null) episode = temp;
            else episode.Complement(temp);
        }

        if (episode?.IsComplete() is not true)
        {
            _logger.LogWarning("未能找到完整的集元数据: {@Query} S{Season}E{Episode}", query, seasonNumber, episodeNumber);
            return null;
        }

        _logger.LogInformation("成功搜索到集: {SeriesName} S{Season}E{Episode}", season.Series.Name, seasonNumber,
            episodeNumber);
        season.AddChild(episode);
        _metadataCache.Set(CacheKeyUtils.GetEpisode(season.Series.OriginalName, seasonNumber, episodeNumber),
            episode);
        _metadataCache.Set(cacheKey, episode);
        return episode;
    }

    public async Task<MovieMetadata?> SearchMovieSingleAsync(SearchQuery query,
        CancellationToken token = default)
    {
        _logger.LogDebug("开始搜索电影: {@Query}", query);
        string cacheKey = CacheKeyUtils.GetMovie(query.Name);
        var cacheAcquireResult = await GetCacheOrWaitAsync<MovieMetadata>(cacheKey, token).ConfigureAwait(false);
        if (cacheAcquireResult.HasMetadata)
        {
            _logger.LogDebug("电影缓存命中: {Name}", query.Name);
            return cacheAcquireResult.Metadata;
        }

        using IFlightLease acquireResultLease = cacheAcquireResult.FlightLease;

        MovieMetadata? movie = null;
        foreach (var provider in GetAvailableProviders())
        {
            if (provider is not IMovieMetadataProvider movieMetadataProvider) continue;
            _logger.LogDebug("使用提供程序 {Provider} 搜索电影: {@Query}", provider.Info.ProviderId, query);
            var temp = await movieMetadataProvider.SearchMovieSingleAsync(query, LANGUAGE_DEFAULT, token)
                .ConfigureAwait(false);
            if (temp is null)
            {
                _logger.LogDebug("提供程序 {Provider} 未找到电影", provider.Info.ProviderId);
                continue;
            }

            if (movie is null) movie = temp;
            else movie.Complement(temp);
        }

        if (movie?.IsComplete() is not true)
        {
            _logger.LogWarning("未能找到完整的电影元数据: {@Query}", query);
            return null;
        }

        _logger.LogInformation("成功搜索到电影: {Name} ({Year})", movie.Name, movie.AirDate?.Year);
        _metadataCache.Set(CacheKeyUtils.GetMovie(movie.OriginalName), movie);
        _metadataCache.Set(cacheKey, movie);
        return movie;
    }

    private async Task<CacheAcquireResult<TMetadata>> GetCacheOrWaitAsync<TMetadata>(
        string cacheKey, CancellationToken token = default) where TMetadata : MetadataBase
    {
        if (_metadataCache.TryGet(cacheKey, out TMetadata? metadata))
        {
            _logger.LogDebug("缓存命中，Key: {CacheKey}", cacheKey);
            return new CacheAcquireResult<TMetadata>(metadata);
        }

        FlightCoordinatorAcquireResult acquireResult;
        do
        {
            acquireResult = await _flightCoordinator.AcquireAsync(cacheKey, token: token).ConfigureAwait(false);
            if (acquireResult.Acquired) break;
            if (!_metadataCache.TryGet(cacheKey, out metadata))
                continue;

            _logger.LogDebug("缓存命中，Key: {CacheKey}", cacheKey);
            return new CacheAcquireResult<TMetadata>(metadata);
        } while (true);

        _logger.LogDebug("获得协调器许可，Key: {CacheKey}", cacheKey);
        return new CacheAcquireResult<TMetadata>(acquireResult.Lease);
    }

    private IEnumerable<IMetadataProvider> GetAvailableProviders()
    {
        return AllProviders
            .Where(x => x.Config.IsEnabled)
            .OrderByDescending(x => x.Config.Priority);
    }

    public MetadataManager(IEnumerable<IMetadataProvider> allProviders, IFlightCoordinator flightCoordinator,
        IMetadataCache metadataCache, ILogger<MetadataManager> logger)
    {
        AllProviders = allProviders;
        _flightCoordinator = flightCoordinator;
        _metadataCache = metadataCache;
        _logger = logger;
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