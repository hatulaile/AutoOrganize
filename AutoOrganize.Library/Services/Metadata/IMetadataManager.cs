using AutoOrganize.Library.Models;
using AutoOrganize.Library.Models.Metadata.Movie;
using AutoOrganize.Library.Models.Metadata.Tv;

namespace AutoOrganize.Library.Services.Metadata;

public interface IMetadataManager
{
    Task<SeriesMetadata?> SearchSeriesSingleAsync(SearchQuery query,
        CancellationToken token = default);

    Task<SeasonMetadata?> SearchSeasonAsync(SearchQuery query, int seasonNumber,
        CancellationToken token = default);

    Task<EpisodeMetadata?> SearchEpisodeAsync(SearchQuery query, int seasonNumber, long episodeNumber,
        CancellationToken token = default);

    Task<MovieMetadata?> SearchMovieSingleAsync(SearchQuery query,
        CancellationToken token = default);
}