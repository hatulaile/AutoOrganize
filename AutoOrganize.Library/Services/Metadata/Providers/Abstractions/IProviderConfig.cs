namespace AutoOrganize.Library.Services.Metadata.Providers.Abstractions;

public interface IProviderConfig
{
    bool IsEnabled { get; }

    int Priority { get; }
}