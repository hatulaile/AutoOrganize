using AutoOrganize.Library.Models.Metadata.Movie;
using AutoOrganize.Library.Models.Metadata.Tv;

namespace AutoOrganize.Library.Services.PathNameGenerators;

public interface IFileNameGenerator
{
    string GetTvSeriesFileName(SeriesMetadata seriesMetadata, ReadOnlySpan<char> pattern = default);

    string GetTvSeasonFileName(SeasonMetadata seasonMetadata, ReadOnlySpan<char> pattern = default);

    string GetTvEpisodeFileName(string path, EpisodeMetadata episodeMetadata, ReadOnlySpan<char> pattern = default);

    string GetMovieFileName(string path, MovieMetadata movieMetadata, ReadOnlySpan<char> pattern = default);

    string GetMovieFolderName(MovieMetadata movieMetadata, ReadOnlySpan<char> pattern = default);
}