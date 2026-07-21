using AutoOrganize.Library.Services.Metadata.Models.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata;

public interface IMetadataFetchService
{
    Task<TResult?> FetchAsync<TRequest, TResult>(TRequest request, CancellationToken token)
        where TRequest : IMetadataRequest<TRequest, TResult>
        where TResult : IMetadataResult<TResult>;

    Task<TResult?> FetchAsync<TRequest, TResult>(TRequest request, bool ignoreCache, CancellationToken token)
        where TRequest : IMetadataRequest<TRequest, TResult>, IHasCache
        where TResult : IMetadataResult<TResult>;
}