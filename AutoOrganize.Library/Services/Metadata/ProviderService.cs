using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Abstractions;
using AutoOrganize.Library.Services.Metadata.Providers.Abstractions;
using Microsoft.Extensions.Logging;

namespace AutoOrganize.Library.Services.Metadata;

public class ProviderService : IProviderService
{
    private readonly ILogger<ProviderService> _logger;
    private readonly List<IProvider> _providers;

    public ProviderService(IEnumerable<IProvider> providers, ILogger<ProviderService> logger)
    {
        _logger = logger;
        _providers = [.. providers];
    }

    public IEnumerable<IProvider> GetProviders(in ProviderFilter filter = ProviderFilter.All) =>
        FilterProviders(_providers, filter);

    public IEnumerable<ISearchProvider<TRequest, TResult>> GetSearchProviders<TRequest, TResult>(
        in ProviderFilter filter = ProviderFilter.All)
        where TRequest : ISearchRequest<TRequest, TResult>
        where TResult : ISearchResult<TResult> =>
        FilterProviders(_providers.OfType<ISearchProvider<TRequest, TResult>>(), filter);

    public IEnumerable<IMetadataProvider<TRequest, TResult>> GetMetadataProviders<TRequest, TResult>(
        in ProviderFilter filter = ProviderFilter.All)
        where TRequest : IMetadataRequest<TRequest, TResult>
        where TResult : IMetadataResult<TResult> =>
        FilterProviders(_providers.OfType<IMetadataProvider<TRequest, TResult>>(), filter);

    public IEnumerable<IProvider> GetProvidersForId(string id, in ProviderFilter filter = ProviderFilter.All) =>
        FilterProviders(_providers, filter).Where(x => x.Info.ProviderId.Equals(id));

    public IEnumerable<TProvider> GetProvidersForId<TProvider>(string id, in ProviderFilter filter = ProviderFilter.All)
        where TProvider : IProvider=>
        FilterProviders(_providers, filter).OfType<TProvider>().Where(x => x.Info.ProviderId.Equals(id));

    private static IEnumerable<TProvider> FilterProviders<TProvider>(in IEnumerable<TProvider> providers,
        in ProviderFilter filter)
        where TProvider : IProvider
    {
        if (filter is ProviderFilter.None)
            return providers;

        var filteredProviders = providers;

        if (filter.HasFlag(filter & ProviderFilter.RespectEnabled))
            filteredProviders = filteredProviders.Where(x => x.Config.IsEnabled);

        if (filter.HasFlag(ProviderFilter.RespectPriority))
            filteredProviders = filteredProviders.OrderByDescending(x => x.Config.Priority);

        return filteredProviders;
    }


    [Flags]
    public enum ProviderFilter
    {
        None = 0,
        RespectEnabled = 1 << 0,
        RespectPriority = 1 << 1,
        All = RespectPriority | RespectEnabled
    }
}