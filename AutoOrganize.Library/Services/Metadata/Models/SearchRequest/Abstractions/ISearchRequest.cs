namespace AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Abstractions;

public interface ISearchRequest
{
}

public interface ISearchRequest<TRequest, TResult> : ISearchRequest
    where TRequest : ISearchRequest<TRequest, TResult>
    where TResult : ISearchResult<TResult>
{
}