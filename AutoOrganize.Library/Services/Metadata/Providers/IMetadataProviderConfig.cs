using AutoOrganize.Library.Services.Config;

namespace AutoOrganize.Library.Services.Metadata.Providers;

public interface IMetadataProviderConfig : IConfig
{
    int Priority => int.MinValue;
}