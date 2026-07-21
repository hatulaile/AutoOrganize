using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Providers.Abstractions;

public interface ISearchProvider<in TRequest, TResult> : IProvider
    where TRequest : ISearchRequest<TRequest, TResult>
    where TResult : ISearchResult<TResult>
{
    Task<IEnumerable<TResult>?> SearchAsync(TRequest request, CancellationToken token = default);
}

public interface IHasCacheSearchProvider<in TRequest, TResult> : ISearchProvider<TRequest, TResult>
    where TRequest : ISearchRequest<TRequest, TResult>
    where TResult : ISearchResult<TResult>
{
    Task<IEnumerable<TResult>?> ISearchProvider<TRequest, TResult>.SearchAsync(TRequest request, CancellationToken token) =>
        SearchAsync(request, false, token);

    Task<IEnumerable<TResult>?> SearchAsync(TRequest request, bool ignoreCache = false, CancellationToken token = default);
}