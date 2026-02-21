using AutoOrganize.Library.Services.Config;

namespace AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbMetadataProviders;

public sealed class ThemoviedbMetadataProviderConfig :
    IConfig<ThemoviedbMetadataProviderConfig>, IMetadataProviderConfig
{
    public int Priority { get; set; }

    public int RateLimitPerSecond { get; set; } = 40;

    public static void Copy(ThemoviedbMetadataProviderConfig target, ThemoviedbMetadataProviderConfig source)
    {
        target.Priority = source.Priority;
        target.RateLimitPerSecond = source.RateLimitPerSecond;
    }
}