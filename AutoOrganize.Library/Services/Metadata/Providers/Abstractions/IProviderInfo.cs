namespace AutoOrganize.Library.Services.Metadata.Providers.Abstractions;

public interface IProviderInfo
{
    string ProviderName { get; }

    string ProviderId { get; }
}