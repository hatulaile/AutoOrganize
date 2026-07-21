using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.Metadata.Models.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata;

public interface IMetadataService
{
    Task<TResult[]?> SearchResultsAsync<TRequest, TResult>(TRequest request, CancellationToken token = default)
        where TResult : ISearchResult<TResult>
        where TRequest : ISearchRequest<TRequest, TResult>;

    Task<TResult[]?> SearchResultsAsync<TRequest, TResult>(TRequest info, bool ignoreCache, CancellationToken token = default)
        where TResult : ISearchResult<TResult>
        where TRequest : ISearchRequest<TRequest, TResult>, IHasCache;

    Task<TResult?> GetMetadataAsync<TRequest, TResult>(TRequest request, CancellationToken token = default)
        where TResult : IMetadataResult<TResult>
        where TRequest : IMetadataRequest<TRequest, TResult>;

    Task<TResult?> GetMetadataAsync<TRequest, TResult>(TRequest request, bool ignoreCache, CancellationToken token = default)
        where TResult : IMetadataResult<TResult>
        where TRequest : IMetadataRequest<TRequest, TResult>, IHasCache;

    Task<TResult?> GetMetadataAsync<TRequest, TResult, TParent>(
        TRequest request, CancellationToken token = default)
        where TResult : IChildOf<TResult, TParent>, IMetadataResult<TResult>
        where TParent : IParentOf<TParent, TResult>, IMetadataResult<TParent>
        where TRequest : IMetadataRequest<TRequest, TResult>, IHasParentRequest;

    Task<TResult?> GetMetadataAsync<TRequest, TResult, TParent>(
        TRequest request, bool ignoreCache, CancellationToken token = default)
        where TResult : IChildOf<TResult, TParent>, IMetadataResult<TResult>
        where TParent : IParentOf<TParent, TResult>, IMetadataResult<TParent>
        where TRequest : IMetadataRequest<TRequest, TResult>, IHasParentRequest, IHasCache;
}