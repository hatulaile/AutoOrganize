using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Providers.Abstractions;

public interface IHasCacheMetadataProvider<in TRequest, TResult> : IMetadataProvider<TRequest, TResult>
    where TRequest : IMetadataRequest<TRequest, TResult>
    where TResult : IMetadataResult<TResult>
{
    Task<TResult?> IMetadataProvider<TRequest, TResult>.GetMetadataAsync(TRequest request, CancellationToken token) =>
        GetMetadataAsync(request, false, token);

    Task<TResult?> GetMetadataAsync(TRequest request, bool ignoreCache = false, CancellationToken token = default);
}