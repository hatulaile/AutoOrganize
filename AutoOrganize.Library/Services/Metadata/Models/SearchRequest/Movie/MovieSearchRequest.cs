using AutoOrganize.Library.Extensions;
using AutoOrganize.Library.Services.Metadata.Models.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.Metadata;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Movie;

public sealed class MovieSearchRequest : ISearchRequest<MovieSearchRequest, MovieSearchResult>, IHasCache
{
    public string? Name { get; set; }

    public int? Year { get; set; }

    public string? Language { get; set; }

    public bool IncludeAdult { get; set; }

    public IProviderIds? ProviderIds { get; set; }

    public IEnumerable<string> GetCacheNames()
    {
        yield return
            $"movie_search_{(ProviderIds is null ? string.Empty : ProviderIds.GetAllProviderCache())}_{Name}_{Year ?? 0}_{Language}";
    }
}