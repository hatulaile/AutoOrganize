namespace AutoOrganize.Library.Services.Metadata.Providers.Abstractions;

public interface IProvider
{
    IProviderInfo Info { get; }

    IProviderConfig Config { get; }
}

public interface IProvider<out TInfo, out TConfig> : IProvider
    where TInfo : IProviderInfo
    where TConfig : IProviderConfig
{
    new TInfo Info { get; }

    new TConfig Config { get; }

    IProviderInfo IProvider.Info => Info;

    IProviderConfig IProvider.Config => Config;
}