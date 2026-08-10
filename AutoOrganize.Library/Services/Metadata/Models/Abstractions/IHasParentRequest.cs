using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Models.Abstractions;

public interface IHasParentRequest
{
    ITypedRequest GetParentRequest();
}

public interface IHasParentRequest<out TParentRequest, TParentResult> : IHasParentRequest
    where TParentRequest : IMetadataRequest<TParentRequest, TParentResult>
    where TParentResult : IMetadataResult<TParentResult>
{
    ITypedRequest IHasParentRequest.GetParentRequest() => GetParentRequest();

    new ITypedRequest<TParentRequest, TParentResult> GetParentRequest();
}