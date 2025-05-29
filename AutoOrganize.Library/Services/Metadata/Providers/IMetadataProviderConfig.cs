using AutoOrganize.Library.Services.Config;

namespace AutoOrganize.Library.Services.Metadata.Providers;

public interface IMetadataProviderConfig : IConfig
{
    bool IsEnabled { get; }

    int Priority => int.MinValue;
}