namespace AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;

internal readonly struct TypedRequest<TRequest, TResult> : ITypedRequest<TRequest, TResult>
    where TRequest : IMetadataRequest<TRequest, TResult>
    where TResult : IMetadataResult<TResult>
{
    public TRequest Request { get; }

    public TypedRequest(TRequest request) => Request = request;

    public async Task<TResult?> FetchAsync(IMetadataFetchService service, CancellationToken token) =>
        await service.FetchAsync<TRequest, TResult>(Request, token).ConfigureAwait(false);
}