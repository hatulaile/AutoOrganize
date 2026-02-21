using AutoOrganize.Library.Models;
using AutoOrganize.Library.Models.Metadata.Images;
using AutoOrganize.Library.Services.RateLimiting;
using AutoOrganize.Library.Utils;
using Nito.Disposables.Internals;
using TMDbLib.Client;
using ImageData = AutoOrganize.Library.Models.Metadata.Images.ImageData;

namespace AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbMetadataProviders;

public sealed partial class ThemoviedbMetadataProvider :
    IMetadataProvider<ThemoviedbMetadataProviderInfo, ThemoviedbMetadataProviderConfig>
{
    private readonly TMDbClient _client;

    private readonly SemaphoreSlim _semaphoreSlim = new(1);

    public ThemoviedbMetadataProviderInfo Info { get; } = new();
    public ThemoviedbMetadataProviderConfig Config { get; }

    private async Task IfNotHasConfigGet(CancellationToken token = default)
    {
        if (_client.HasConfig) return;
        try
        {
            await _semaphoreSlim.WaitAsync(token).ConfigureAwait(false);
            if (_client.HasConfig) return;
            await _client.GetConfigAsync().WaitAsync(token).ConfigureAwait(false);
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
            ImageUrl = _client.GetImageUrl("original", imageData.FilePath).AbsoluteUri,
            AspectRatio = imageData.AspectRatio,
            Height = imageData.Height,
            Width = imageData.Width,
            Locale = LocaleUtils.GetCultureInfo(imageData.Iso_639_1, imageData.Iso_3166_1),
            Priority = imageData.VoteCount
        };
    }

    public ThemoviedbMetadataProvider(TMDbClient client, ThemoviedbMetadataProviderConfig config)
    {
        _client = client;
        Config = config;
    }
}