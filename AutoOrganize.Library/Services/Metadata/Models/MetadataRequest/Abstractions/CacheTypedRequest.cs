using AutoOrganize.Library.Services.Metadata.Models.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;

internal readonly struct CacheTypedRequest<TRequest, TResult> : ICacheTypedRequest<TRequest, TResult>
    where TRequest : IMetadataRequest<TRequest, TResult>, IHasCache
    where TResult : IMetadataResult<TResult>
{
    public TRequest Request { get; }

    public CacheTypedRequest(TRequest request) => Request = request;

    public Task<TResult?> FetchAsync(IMetadataFetchService service, bool ignoreCache, CancellationToken token) =>
        service.FetchAsync<TRequest, TResult>(Request, ignoreCache, token);
}