using AutoOrganize.Library.Services.Metadata.Models.Metadata;

namespace AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Abstractions;

public interface ISearchResult
{
    public IProviderIds ProviderIds { get; }
}

public interface ISearchResult<TSelf> : ISearchResult
    where TSelf : ISearchResult<TSelf>
{
}