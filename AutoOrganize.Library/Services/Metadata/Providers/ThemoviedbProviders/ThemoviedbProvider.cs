using System.Diagnostics.CodeAnalysis;
using AutoOrganize.Library.Services.Config;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Images;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Movie;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Movie;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Tv;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Movie;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Tv;
using AutoOrganize.Library.Services.Metadata.Providers.Abstractions;
using AutoOrganize.Library.Utils;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Nito.Disposables.Internals;
using TMDbLib.Client;
using ImageData = AutoOrganize.Library.Services.Metadata.Models.Metadata.Images.ImageData;

namespace AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbProviders;

public sealed partial class ThemoviedbProvider :
    IProvider<ThemoviedbProviderInfo, ThemoviedbProviderConfig>,
    IMovieProvider,
    ISeriesProvider,
    ISeasonProvider,
    IEpisodeProvider,
    IHasCacheSearchProvider<MovieSearchRequest, MovieSearchResult>,
    IHasCacheSearchProvider<SeriesSearchRequest, SeriesSearchResult>,
    IHasCacheMetadataProvider<MovieMetadataRequest, MovieMetadata>,
    IHasCacheMetadataProvider<SeriesMetadataRequest, SeriesMetadata>,
    IHasCacheMetadataProvider<SeasonMetadataRequest, SeasonMetadata>,
    IHasCacheMetadataProvider<EpisodeMetadataRequest, EpisodeMetadata>
{
    private readonly ILogger<ThemoviedbProvider> _logger;
    private TMDbClient Client { get; set; }

    private readonly SemaphoreSlim _semaphoreSlim = new(1);

    private static readonly TimeSpan CacheTime = TimeSpan.FromMinutes(5);
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

    public ThemoviedbProviderInfo Info { get; }
    public ThemoviedbProviderConfig Config { get; }

    public ThemoviedbProvider(IFileConfigManager fileConfigManager, ILogger<ThemoviedbProvider> logger)
    {
        _logger = logger;
        Info = new ThemoviedbProviderInfo();
        Config = fileConfigManager.GetConfigOrLoad<ThemoviedbProviderConfig>();
        //这里使用了 api.tmdb.org, 国内访问默认的 api.themoviedb.org 会有问题);
        Client = new TMDbClient(Config.ApiKey, baseUrl: "api.tmdb.org");
        Config.WeakReferenceMessenger.Register<ThemoviedbProvider, ThemoviedbProviderConfig.ApiKeyChangedMessage>(this,
            HandleApiKeyChanged);
    }

    private static void HandleApiKeyChanged(ThemoviedbProvider provider,
        ThemoviedbProviderConfig.ApiKeyChangedMessage msg)
    {
        Task.Run(async () =>
        {
            if (provider.Client.ApiKey.Equals(msg.NewValue))
            {
                provider._logger.LogDebug("新 api key 与当前使用的相同, 未更换");
                return;
            }

            provider._logger.LogDebug("开始更换 api key 流程");
            bool hasSlim = false;

            TMDbClient? client = null;
            try
            {
                await provider._semaphoreSlim.WaitAsync().ConfigureAwait(false);
                hasSlim = true;
                client = new TMDbClient(msg.NewValue, baseUrl: "api.tmdb.org");
                provider._logger.LogDebug("更换 api key, 等待新客户端获取配置");
                await client.GetConfigAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                client = null;
                provider._logger.LogError(e, "Failed to update TMDB client with new API key.");
            }
            finally
            {
                if (client is not null)
                {
                    provider._logger.LogDebug("更换 api key, 正在替换");
                    TMDbClient oldClient = provider.Client;
                    provider.Client = client;
                    oldClient.Dispose();
                }
                else
                {
                    provider.Config.ApiKey = provider.Client.ApiKey;
                }

                if (hasSlim)
                    provider._semaphoreSlim.Release();
            }
        });
    }

    private static string GetSearchCacheKey(string name, int? year, string? language) =>
        $"search_{name}_{year}_{language}";

    private async Task IfNotHasConfigGet(CancellationToken token = default)
    {
        if (Client.HasConfig) return;
        try
        {
            await _semaphoreSlim.WaitAsync(token).ConfigureAwait(false);
            if (Client.HasConfig) return;
            await Client.GetConfigAsync().WaitAsync(token).ConfigureAwait(false);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration",Justification = "这里多次枚举开销不大，抑制警告！")]
    private ImageGroup? ImageDataListToGroup(IEnumerable<TMDbLib.Objects.General.ImageData>? imageDatas)
    {
        if (imageDatas is null)
            return null;

        var images = imageDatas.Select(ImageDataToData).WhereNotNull();
        if (images.Any())
            return null;

        return new ImageGroup(new ThemoviedbImageDataList(images));
    }

    private ImageData? ImageDataToData(TMDbLib.Objects.General.ImageData imageData)
    {
        if (imageData.FilePath is null)
            return null;

        return new ImageData
        {
            ImageUrl = Client.GetImageUrl("original", imageData.FilePath),
            AspectRatio = imageData.AspectRatio,
            Height = imageData.Height,
            Width = imageData.Width,
            Locale = LocaleUtils.GetCultureInfo(imageData.Iso_639_1, imageData.Iso_3166_1),
            Priority = imageData.VoteCount
        };
    }
}
