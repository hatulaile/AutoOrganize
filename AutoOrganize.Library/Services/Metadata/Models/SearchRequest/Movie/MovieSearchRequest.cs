using AutoOrganize.Library.Services.Metadata.Models.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Movie;

public sealed class MovieSearchRequest : ISearchRequest<MovieSearchRequest, MovieSearchResult>, IHasCache
{
    public string Name { get; init; } = string.Empty;

    public int? Year { get; init; }

    public string? Language { get; init; }

    public bool IncludeAdult { get; init; }

    public IEnumerable<string> GetCacheNames()
    {
        yield return $"movie_search_{Name}_{Year ?? 0}_{Language}";
    }
}
