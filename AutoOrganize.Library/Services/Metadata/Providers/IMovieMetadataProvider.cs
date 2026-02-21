using AutoOrganize.Library.Models;
using AutoOrganize.Library.Models.Metadata.Movie;

namespace AutoOrganize.Library.Services.Metadata.Providers;

public interface IMovieMetadataProvider : IMetadataProvider
{
    Task<IEnumerable<MovieMetadata>> SearchMovieAsync(SearchQuery query, string? language = null,
        CancellationToken token = default);

    Task<MovieMetadata?> SearchMovieSingleAsync(SearchQuery query, string? language = null,
        CancellationToken token = default);

    Task<MovieMetadata?> GetMovieAsync(string id, string? language = null,
        CancellationToken token = default);
}