using AutoOrganize.Library.Models;
using AutoOrganize.Library.Models.Metadata.Images;
using AutoOrganize.Library.Services.Config;
using AutoOrganize.Library.Utils;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Nito.Disposables.Internals;
using TMDbLib.Client;
using ImageData = AutoOrganize.Library.Models.Metadata.Images.ImageData;

namespace AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbMetadataProviders;

public sealed partial class ThemoviedbMetadataProvider :
    IMetadataProvider<ThemoviedbMetadataProviderInfo, ThemoviedbMetadataProviderConfig>
{
    private readonly ILogger<ThemoviedbMetadataProvider> _logger;
    private TMDbClient Client { get; set; }

    private readonly SemaphoreSlim _semaphoreSlim = new(1);

    public ThemoviedbMetadataProviderInfo Info { get; } = new();
    public ThemoviedbMetadataProviderConfig Config { get; }

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

    private ImageGroup? ImageDataListToGroup(IEnumerable<TMDbLib.Objects.General.ImageData>? imageDatas)
    {
        if (imageDatas is null)
            return null;

        var images = imageDatas.Select(ImageDataToData).WhereNotNull().ToArray();
        if (images.Length == 0)
            return null;

        return new ImageGroup(new MetadataProviderImageDataList(MetadataProviderType.ThemovieDB, images));
    }

    private ImageData? ImageDataToData(TMDbLib.Objects.General.ImageData imageData)
    {
        if (imageData.FilePath is null)
            return null;

        return new ImageData
        {
            ImageUrl = Client.GetImageUrl("original", imageData.FilePath).AbsoluteUri,
            AspectRatio = imageData.AspectRatio,
            Height = imageData.Height,
            Width = imageData.Width,
            Locale = LocaleUtils.GetCultureInfo(imageData.Iso_639_1, imageData.Iso_3166_1),
            Priority = imageData.VoteCount
        };
    }

    public ThemoviedbMetadataProvider(IFileConfigManager fileConfigManager, ILogger<ThemoviedbMetadataProvider> logger)
    {
        _logger = logger;
        Config = fileConfigManager.GetConfigOrLoad<ThemoviedbMetadataProviderConfig>();
        //这里使用了 api.tmdb.org, 国内访问默认的 api.themoviedb.org 会有问题);
        Client = new TMDbClient(Config.ApiKey, baseUrl: "api.tmdb.org");
        Config.WeakReferenceMessenger.Register(this,
            static (ThemoviedbMetadataProvider provider, ThemoviedbMetadataProviderConfig.ApiKeyChangedMessage msg) =>
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
            });
    }
}