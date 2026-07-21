using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Images;

namespace AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbProviders;

public sealed class ThemoviedbImageDataList : ImageDataList
{
    public override string Id => nameof(ProviderType.ThemovieDB);

    public ThemoviedbImageDataList()
    {
    }

    public ThemoviedbImageDataList(int capacity) : base(capacity)
    {
    }

    public ThemoviedbImageDataList(IEnumerable<ImageData> images) : base(images)
    {
    }
}