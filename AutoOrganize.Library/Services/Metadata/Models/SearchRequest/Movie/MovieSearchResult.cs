using AutoOrganize.Library.Services.Metadata.Models.Metadata;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Abstractions;

namespace AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Movie;

public sealed class MovieSearchResult : ISearchResult<MovieSearchResult>
{
    public string? Title { get; set; }

    public bool Adult { get; set; }

    public string? OriginalTitle { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public IProviderIds? ProviderIds { get; set; }
}
