using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Providers.Abstractions;

public interface IMetadataProvider<in TRequest, TResult> : IProvider
    where TRequest : IMetadataRequest<TRequest, TResult>
    where TResult : IMetadataResult<TResult>
{
    Task<TResult?> GetMetadataAsync(TRequest request, CancellationToken token = default);
}