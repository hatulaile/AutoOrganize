using AutoOrganize.Library.Exceptions;
using AutoOrganize.Library.Models.Metadata.Movie;
using AutoOrganize.Library.Models.Metadata.Tv;
using AutoOrganize.Library.Services.PathNameGenerators.Options;
using AutoOrganize.Library.Utils;

namespace AutoOrganize.Library.Services.PathNameGenerators;

public sealed class PathNameGenerator : IPathNameGenerator
{
    public const string TV_SERIES_PATTERN = "{sn}.{year}";
    public const string TV_SEASON_PATTERN = "Season {s00}";
    public const string TV_EPISODE_PATTERN = "{sn}.S{s00}E{e00} - {en}.{ext}";
    public const string MOVIE_PATTERN = "{name}.{year}.{ext}";
    public const string MOVIE_FOLDER_PATTERN = "{name}.{year}";

    public string GetTvSeriesFileName(SeriesMetadata seriesMetadata, TvFileNameGenerationOptions? option = null)
    {
        string pattern = option?.SeriesMetadataFolderPattern ?? TV_SERIES_PATTERN;

        var newFileName = pattern
            .Replace("{sn}", GetValidName(seriesMetadata.Name), StringComparison.OrdinalIgnoreCase)
            .Replace("{son}", GetValidName(seriesMetadata.OriginalName), StringComparison.OrdinalIgnoreCase)
            .Replace("{year}", seriesMetadata.AirDate?.Year.ToString(), StringComparison.OrdinalIgnoreCase);

        return PathUtils.GetValidFileName(newFileName);
    }

    public string GetTvSeasonFileName(SeasonMetadata seriesMetadata, TvFileNameGenerationOptions? option = null)
    {
        string pattern = option?.SeasonMetadataFolderPattern ?? TV_SEASON_PATTERN;

        int seasonNumber = seriesMetadata.SeasonNumber
                           ?? throw new MetadataFieldNullException(nameof(SeasonMetadata),
                               nameof(seriesMetadata.SeasonNumber));

        var newFileName = pattern
            .Replace("{sn}", GetValidName(seriesMetadata.Series?.Name), StringComparison.OrdinalIgnoreCase)
            .Replace("{son}", GetValidName(seriesMetadata.Series?.OriginalName), StringComparison.OrdinalIgnoreCase)
            .Replace("{snn}", GetValidName(seriesMetadata.Name), StringComparison.OrdinalIgnoreCase)
            .Replace("{year}", seriesMetadata.AirDate?.Year.ToString(), StringComparison.OrdinalIgnoreCase);

        int startIndex = newFileName.IndexOf('{');
        while (startIndex != -1)
        {
            int endIndex = newFileName.IndexOf('}', startIndex);
            if (endIndex == -1) break;
            string sub = newFileName.Substring(startIndex + 1, endIndex - startIndex - 1);
            if (sub[0].Equals('s') || sub[0].Equals('S'))
            {
                newFileName = StringUtils.ReplaceRange(newFileName, startIndex, endIndex - startIndex + 1,
                    seasonNumber.ToString().PadLeft(endIndex - startIndex - 2, '0'));
            }

            startIndex = newFileName.IndexOf('{');
        }

        return PathUtils.GetValidFileName(newFileName);
    }

    public string GetTvEpisodeFileName(string path, EpisodeMetadata episodeMetadata,
        TvFileNameGenerationOptions? option = null)
    {
        string pattern = option?.EpisodeNamePattern ?? TV_EPISODE_PATTERN;

        int seasonNumber = episodeMetadata.SeasonNumber
                           ?? throw new MetadataFieldNullException(nameof(EpisodeMetadata),
                               nameof(episodeMetadata.SeasonNumber));

        long episodeNumber = episodeMetadata.EpisodeNumber
                             ?? throw new MetadataFieldNullException(nameof(EpisodeMetadata),
                                 nameof(episodeMetadata.EpisodeNumber));

        var newFileName = pattern
            .Replace("{sn}", GetValidName(episodeMetadata.Series?.Name), StringComparison.OrdinalIgnoreCase)
            .Replace("{son}", GetValidName(episodeMetadata.Series?.OriginalName), StringComparison.OrdinalIgnoreCase)
            .Replace("{snn}", GetValidName(episodeMetadata.Season?.Name), StringComparison.OrdinalIgnoreCase)
            .Replace("{en}", GetValidName(episodeMetadata.Name), StringComparison.OrdinalIgnoreCase)
            .Replace("{year}", episodeMetadata.AirDate?.Year.ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("{ext}", Path.GetExtension(path).TrimStart('.'), StringComparison.OrdinalIgnoreCase)
            .Replace("{fn}", Path.GetFileNameWithoutExtension(path), StringComparison.OrdinalIgnoreCase);

        int startIndex = newFileName.IndexOf('{');
        while (startIndex != -1)
        {
            int endIndex = newFileName.IndexOf('}', startIndex);
            if (endIndex == -1) break;
            string sub = newFileName.Substring(startIndex + 1, endIndex - startIndex - 1);
            if (sub[0].Equals('s') || sub[0].Equals('S'))
            {
                newFileName = StringUtils.ReplaceRange(newFileName, startIndex, endIndex - startIndex + 1,
                    seasonNumber.ToString().PadLeft(endIndex - startIndex - 2, '0'));
            }
            else if (sub[0].Equals('e') || sub[0].Equals('E'))
            {
                newFileName = StringUtils.ReplaceRange(newFileName, startIndex, endIndex - startIndex + 1,
                    episodeNumber.ToString().PadLeft(endIndex - startIndex - 2, '0'));
            }

            startIndex = newFileName.IndexOf('{');
        }

        return PathUtils.GetValidFileName(newFileName);
    }

    public string GetMovieFileName(string path, MovieMetadata movieMetadata,
        MovieFileNameGenerationOptions? option = null)
    {
        string pattern = option?.MoviePattern ?? MOVIE_PATTERN;
        var newFileName = pattern
            .Replace("{name}", GetValidName(movieMetadata.Name))
            .Replace("{oname}", GetValidName(movieMetadata.OriginalName))
            .Replace("{year}", movieMetadata.AirDate?.Year.ToString() ?? "Unknown")
            .Replace("{ext}", Path.GetExtension(path).TrimStart('.'))
            .Replace("{fn}", Path.GetFileNameWithoutExtension(path));
        return PathUtils.GetValidFileName(newFileName);
    }

    public string GetMovieFolderName(MovieMetadata movieMetadata,
        MovieFileNameGenerationOptions? option = null)
    {
        string pattern = option?.MoviePattern ?? MOVIE_FOLDER_PATTERN;
        var newFileName = pattern
            .Replace("{name}", GetValidName(movieMetadata.Name))
            .Replace("{oname}", GetValidName(movieMetadata.OriginalName))
            .Replace("{year}", movieMetadata.AirDate?.Year.ToString() ?? "Unknown");
        return PathUtils.GetValidFileName(newFileName);
    }

    private string GetValidName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        return PathUtils.GetValidFileName(name, ' ');
    }
}