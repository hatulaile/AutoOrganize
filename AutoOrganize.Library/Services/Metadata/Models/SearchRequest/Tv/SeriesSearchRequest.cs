using AutoOrganize.Library.Services.Metadata.Models.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Tv;

public sealed class SeriesSearchRequest : ISearchRequest<SeriesSearchRequest, SeriesSearchResult>, IHasCache
{
    public string Name { get; init; } = string.Empty;

    public int? FirstAirDateYear { get; init; }

    public string? Language { get; init; }

    public IEnumerable<string> GetCacheNames()
    {
        yield return $"series_{Name}_{FirstAirDateYear}_{Language}";
    }
}