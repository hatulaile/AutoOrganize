using AutoOrganize.Library.Models.Metadata.Movie;
using AutoOrganize.Library.Models.Metadata.Tv;
using AutoOrganize.Library.Services.PathNameGenerators.Options;

namespace AutoOrganize.Library.Services.PathNameGenerators;

public interface IPathNameGenerator
{
    string GetTvSeriesFileName(SeriesMetadata seriesMetadata,
        in TvFileNameGenerationOptions options = default);

    string GetTvSeasonFileName(SeasonMetadata seasonMetadata,
        in TvFileNameGenerationOptions options = default);

    string GetTvEpisodeFileName(string path, EpisodeMetadata episodeMetadata,
        in TvFileNameGenerationOptions options = default);

    string GetMovieFileName(string path, MovieMetadata movieMetadata,
        in MovieFileNameGenerationOptions option = default);

    string GetMovieFolderName(MovieMetadata movieMetadata,
        in MovieFileNameGenerationOptions option = default);
}