using AutoOrganize.Library.Models.Metadata.Movie;
using AutoOrganize.Library.Models.Metadata.Tv;

namespace AutoOrganize.Library.Services.PathNameGenerators;

public interface IFileNameGenerator
{
    string GetTvSeriesFileName(SeriesMetadata seriesMetadata, string? pattern = null);

    string GetTvSeasonFileName(SeasonMetadata seasonMetadata, string? pattern = null);

    string GetTvEpisodeFileName(string path, EpisodeMetadata episodeMetadata, string? pattern = null);

    string GetMovieFileName(string path, MovieMetadata movieMetadata, string? pattern = null);

    string GetMovieFolderName(MovieMetadata movieMetadata, string? pattern = null);
}