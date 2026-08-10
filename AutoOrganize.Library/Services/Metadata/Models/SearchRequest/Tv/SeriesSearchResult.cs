using System.Globalization;
using AutoOrganize.Library.Services.Metadata.Models.Metadata;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Tv;

public sealed class SeriesSearchResult : ISearchResult<SeriesSearchResult>
{
    public string? Name { get; set; }

    public string? OriginalName { get; set; }

    public List<RegionInfo>? OriginCountry { get; set; }

    public DateTime? FirstAirDate { get; set; }

    public IProviderIds ProviderIds { get; set; } = new ProviderIds();
}
