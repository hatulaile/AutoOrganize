using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Abstractions;
using AutoOrganize.Library.Services.Metadata.Providers;
using AutoOrganize.Library.Services.Metadata.Providers.Abstractions;

namespace AutoOrganize.Library.Services.Metadata;

public interface IProviderService
{
    IEnumerable<IProvider> GetProviders(in ProviderService.ProviderFilter filter = ProviderService.ProviderFilter.All);

    IEnumerable<ISearchProvider<TRequest, TResult>> GetSearchProviders<TRequest, TResult>(
        in ProviderService.ProviderFilter filter = ProviderService.ProviderFilter.All)
        where TRequest : ISearchRequest<TRequest, TResult>
        where TResult : ISearchResult<TResult>;

    IEnumerable<IMetadataProvider<TRequest, TResult>> GetMetadataProviders<TRequest, TResult>(
        in ProviderService.ProviderFilter filter = ProviderService.ProviderFilter.All)
        where TRequest : IMetadataRequest<TRequest, TResult>
        where TResult : IMetadataResult<TResult>;

    IEnumerable<IProvider> GetProvidersForId(string id, in ProviderService.ProviderFilter filter = ProviderService.ProviderFilter.All);

    IEnumerable<TProvider> GetProvidersForId<TProvider>(string id, in ProviderService.ProviderFilter filter = ProviderService.ProviderFilter.All)
        where TProvider : IProvider;
}