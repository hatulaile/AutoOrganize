namespace AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;

public interface IMetadataRequest
{
}

public interface IMetadataRequest<TRequest, TResult> : IMetadataRequest
    where TRequest : IMetadataRequest<TRequest, TResult>
    where TResult : IMetadataResult<TResult>
{
}