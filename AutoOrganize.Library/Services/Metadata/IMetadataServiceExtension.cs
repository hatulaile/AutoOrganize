using AutoOrganize.Library.Services.Metadata.Models.Metadata.Movie;
using AutoOrganize.Library.Services.Metadata.Models.Metadata.Tv;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Movie;
using AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Tv;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Movie;
using AutoOrganize.Library.Services.Metadata.Models.SearchRequest.Tv;

namespace AutoOrganize.Library.Services.Metadata;

public static class MetadataServiceExtension
{
    extension(IMetadataService service)
    {
        public async Task<IEnumerable<MovieSearchResult>?> SearchMovieAsync(MovieSearchRequest request,
            CancellationToken token = default) =>
            await service.SearchResultsAsync<MovieSearchRequest, MovieSearchResult>(request, token)
                .ConfigureAwait(false);

        public Task<MovieMetadata?> GetMovieAsync(MovieMetadataRequest request,
            CancellationToken token = default) =>
            service.GetMetadataAsync<MovieMetadataRequest, MovieMetadata>(request, token);

        public async Task<IEnumerable<SeriesSearchResult>?> SearchSeriesAsync(SeriesSearchRequest request,
            CancellationToken token = default) =>
            await service.SearchResultsAsync<SeriesSearchRequest, SeriesSearchResult>(request, token)
                .ConfigureAwait(false);

        public Task<SeriesMetadata?> GetSeriesAsync(SeriesMetadataRequest request,
            CancellationToken token = default) =>
            service.GetMetadataAsync<SeriesMetadataRequest, SeriesMetadata>(request, token);

        public Task<SeasonMetadata?> GetSeasonAsync(SeasonMetadataRequest request,
            CancellationToken token = default) =>
            service.GetMetadataAsync<SeasonMetadataRequest, SeasonMetadata, SeriesMetadata>(request, token);

        public Task<EpisodeMetadata?> GetEpisodeAsync(EpisodeMetadataRequest request,
            CancellationToken token = default) =>
            service.GetMetadataAsync<EpisodeMetadataRequest, EpisodeMetadata, SeasonMetadata>(request, token);
    }
}
