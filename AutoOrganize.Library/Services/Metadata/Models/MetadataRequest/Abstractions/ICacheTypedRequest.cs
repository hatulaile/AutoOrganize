using AutoOrganize.Library.Services.Metadata.Models.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;

public interface ICacheTypedRequest : ITypedRequest
{
    Task<object?> FetchAsync(IMetadataFetchService service, bool ignoreCache, CancellationToken token);
}

public interface ICacheTypedRequest<out TRequest, TResult> : ITypedRequest<TRequest, TResult>, ICacheTypedRequest
    where TRequest : IMetadataRequest<TRequest, TResult>, IHasCache
    where TResult : IMetadataResult<TResult>
{
    new Task<TResult?> FetchAsync(IMetadataFetchService service, bool ignoreCache, CancellationToken token);

    async Task<object?>
        ICacheTypedRequest.FetchAsync(IMetadataFetchService service, bool ignoreCache, CancellationToken token) =>
        await FetchAsync(service, ignoreCache, token).ConfigureAwait(false);

    async Task<TResult?>
        ITypedRequest<TRequest, TResult>.FetchAsync(IMetadataFetchService service, CancellationToken token) =>
        await service.FetchAsync<TRequest, TResult>(Request, false, token).ConfigureAwait(false);
}