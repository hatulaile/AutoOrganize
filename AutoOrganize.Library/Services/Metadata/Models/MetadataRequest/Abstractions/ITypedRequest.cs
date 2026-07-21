using AutoOrganize.Library.Services.Metadata;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;

public interface ITypedRequest
{
    IMetadataRequest Request { get; }

    Task<object?> FetchAsync(IMetadataFetchService service, CancellationToken token);
}

public interface ITypedRequest<out TRequest, TResult> : ITypedRequest
    where TRequest : IMetadataRequest<TRequest, TResult>
    where TResult : IMetadataResult<TResult>
{
    IMetadataRequest ITypedRequest.Request => Request;

    async Task<object?> ITypedRequest.FetchAsync(IMetadataFetchService service, CancellationToken token) =>
        await FetchAsync(service, token).ConfigureAwait(false);

    new TRequest Request { get; }

    new Task<TResult?> FetchAsync(IMetadataFetchService service, CancellationToken token);
}