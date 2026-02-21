namespace AutoOrganize.Library.Services.Metadata.Providers;

public interface IMetadataProvider
{
    IMetadataProviderInfo Info { get; }

    IMetadataProviderConfig Config { get; }
}

public interface IMetadataProvider<out TInfo> : IMetadataProvider
    where TInfo : IMetadataProviderInfo
{
    new TInfo Info { get; }

    IMetadataProviderInfo IMetadataProvider.Info => Info;

}

public interface IMetadataProvider<out TInfo, out TConfig> : IMetadataProvider<TInfo>
    where TInfo : IMetadataProviderInfo
    where TConfig : IMetadataProviderConfig
{
    new TInfo Info { get; }

    new TConfig Config { get; }

    IMetadataProviderInfo IMetadataProvider.Info => Info;

    IMetadataProviderConfig IMetadataProvider.Config => Config;
}