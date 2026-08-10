using AutoOrganize.Library.Extensions;
using AutoOrganize.Library.Services.Metadata.Models.Abstractions;
using AutoOrganize.Library.Services.Metadata.Models.Metadata;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Tv;

public sealed class SeriesSearchRequest : ISearchRequest<SeriesSearchRequest, SeriesSearchResult>, IHasCache
{
    public string? Name { get; set; }

    public int? FirstAirDateYear { get; set; }

    public string? Language { get; set; }

    public IProviderIds? ProviderIds { get; set; }

    public IEnumerable<string> GetCacheNames()
    {
        yield return
            $"series_{Name}_{(ProviderIds is null ? string.Empty : ProviderIds.GetAllProviderCache())}_{FirstAirDateYear}_{Language}";
    }
}