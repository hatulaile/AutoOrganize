using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Models.Abstractions;

public interface IHasParentRequest
{
    ITypedRequest GetParentRequest();
}

public interface IHasParentRequest<TParentRequest> : IHasParentRequest
    where TParentRequest : IMetadataRequest
{
    ITypedRequest IHasParentRequest.GetParentRequest() => GetParentRequest();

    new ITypedRequest GetParentRequest();
}