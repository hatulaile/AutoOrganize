using AutoOrganize.Library.Models;
using AutoOrganize.Library.Models.Metadata.Tv;

namespace AutoOrganize.Library.Services.Metadata.Providers;

public interface ITvMetadataProvider : IMetadataProvider
{
    Task<IEnumerable<SeriesMetadata>> SearchSeriesAsync(SearchQuery query, string? language = null,
        CancellationToken token = default);

    Task<SeriesMetadata?> SearchSeriesSingleAsync(SearchQuery query, string? language = null,
        CancellationToken token = default);

    Task<SeriesMetadata?> GetSeriesAsync(string id, string? language = null,
        CancellationToken token = default);

    Task<SeasonMetadata?> GetSeasonMetadataAsync(string id, int seasonNumber, string? language = null,
        CancellationToken token = default);

    Task<EpisodeMetadata?> GetEpisodeMetadataAsync(string id, int seasonNumber, long episodeNumber,
        string? language = null, CancellationToken token = default);
}