using AutoOrganize.Library.Models.Metadata.Movie;
using AutoOrganize.Library.Models.Metadata.Tv;
using AutoOrganize.Library.Services.PathNameGenerators.Options;

namespace AutoOrganize.Library.Services.PathNameGenerators;

public interface IPathNameGenerator
{
    string GetTvSeriesFileName(SeriesMetadata seriesMetadata,
        TvFileNameGenerationOptions? options = null);

    string GetTvSeasonFileName(SeasonMetadata seasonMetadata,
        TvFileNameGenerationOptions? options = null);

    string GetTvEpisodeFileName(string path, EpisodeMetadata episodeMetadata,
        TvFileNameGenerationOptions? options = null);

    string GetMovieFileName(string path, MovieMetadata movieMetadata,
        MovieFileNameGenerationOptions? option = null);

    string GetMovieFolderName(MovieMetadata movieMetadata,
        MovieFileNameGenerationOptions? option = null);
}