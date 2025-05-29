using AutoOrganize.Library.Exceptions;
using AutoOrganize.Library.Models.Metadata.Movie;
using AutoOrganize.Library.Models.Metadata.Tv;
using AutoOrganize.Library.Utils;
using Microsoft.Extensions.Logging;

namespace AutoOrganize.Library.Services.PathNameGenerators;

public sealed class FileNameGenerator : IFileNameGenerator
{
    private readonly ILogger<FileNameGenerator> _logger;
    public const string TV_SERIES_PATTERN = "{sn}.{year}";
    public const string TV_SEASON_PATTERN = "Season {s00}";
    public const string TV_EPISODE_PATTERN = "{sn}.S{s00}E{e00} - {en}.{ext}";
    public const string MOVIE_PATTERN = "{name}.{year}.{ext}";
    public const string MOVIE_FOLDER_PATTERN = "{name}.{year}";

    public string GetTvSeriesFileName(SeriesMetadata seriesMetadata, string? pattern = null)
    {
        pattern ??= TV_SERIES_PATTERN;
        _logger.LogDebug("生成剧集文件夹名 模板: {Pattern}, 剧集名: {Name}, 原始名: {OriginalName}, 年份: {Year}",
            pattern, seriesMetadata.Name, seriesMetadata.OriginalName, seriesMetadata.AirDate?.Year);

        var newFileName = pattern
            .Replace("{sn}", GetValidName(seriesMetadata.Name), StringComparison.OrdinalIgnoreCase)
            .Replace("{son}", GetValidName(seriesMetadata.OriginalName), StringComparison.OrdinalIgnoreCase)
            .Replace("{year}", seriesMetadata.AirDate?.Year.ToString(), StringComparison.OrdinalIgnoreCase);


        var result = PathUtils.GetValidFileName(newFileName);
        _logger.LogDebug("剧集名: {Name} 生成结果: {Result}", seriesMetadata.Name, result);
        return result;
    }

    public string GetTvSeasonFileName(SeasonMetadata seasonMetadata, string? pattern = null)
    {
        pattern ??= TV_SEASON_PATTERN;
        int seasonNumber = seasonMetadata.SeasonNumber
                           ?? throw new MetadataFieldNullException(nameof(SeasonMetadata),
                               nameof(seasonMetadata.SeasonNumber));
        _logger.LogDebug("生成季文件夹名 模板: {Pattern}, 剧集: {SeriesName}, S{Season}",
            pattern, seasonMetadata.Series?.Name, seasonNumber);

        var newFileName = pattern
            .Replace("{sn}", GetValidName(seasonMetadata.Series?.Name), StringComparison.OrdinalIgnoreCase)
            .Replace("{son}", GetValidName(seasonMetadata.Series?.OriginalName), StringComparison.OrdinalIgnoreCase)
            .Replace("{snn}", GetValidName(seasonMetadata.Name), StringComparison.OrdinalIgnoreCase)
            .Replace("{year}", seasonMetadata.AirDate?.Year.ToString(), StringComparison.OrdinalIgnoreCase);

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

            startIndex = newFileName.IndexOf('{', startIndex + 1);
        }

        string result = PathUtils.GetValidFileName(newFileName);
        _logger.LogDebug("季: {Name} S{SeasonNumber} 生成结果: {Result}", seasonMetadata.Series?.Name, seasonNumber, result);
        return result;
    }

    public string GetTvEpisodeFileName(string path, EpisodeMetadata episodeMetadata, string? pattern = null)
    {
        pattern ??= TV_EPISODE_PATTERN;

        int seasonNumber = episodeMetadata.SeasonNumber
                           ?? throw new MetadataFieldNullException(nameof(EpisodeMetadata),
                               nameof(episodeMetadata.SeasonNumber));

        long episodeNumber = episodeMetadata.EpisodeNumber
                             ?? throw new MetadataFieldNullException(nameof(EpisodeMetadata),
                                 nameof(episodeMetadata.EpisodeNumber));

        _logger.LogDebug("生成剧集文件名 模板: {Pattern}, 剧集: {SeriesName} S{Season}E{Episode}, 分集名: {EpisodeName}",
            pattern, episodeMetadata.Series?.Name, seasonNumber, episodeNumber, episodeMetadata.Name);

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

            startIndex = newFileName.IndexOf('{', startIndex + 1);
        }

        string result = PathUtils.GetValidFileName(newFileName);
        _logger.LogDebug("季: {Name} S{SeasonNumber}E{EpisodeNumber} 生成结果: {Result}", episodeMetadata.Series?.Name,
            seasonNumber, episodeNumber, result);
        return result;
    }

    public string GetMovieFileName(string path, MovieMetadata movieMetadata, string? pattern = null)
    {
        pattern ??= MOVIE_PATTERN;
        _logger.LogDebug("生成电影文件名 模板: {Pattern}, 电影:{Name}, 年份={Year}",
            pattern, movieMetadata.Name, movieMetadata.AirDate?.Year);

        var newFileName = pattern
            .Replace("{name}", GetValidName(movieMetadata.Name))
            .Replace("{oname}", GetValidName(movieMetadata.OriginalName))
            .Replace("{year}", movieMetadata.AirDate?.Year.ToString() ?? "Unknown")
            .Replace("{ext}", Path.GetExtension(path).TrimStart('.'))
            .Replace("{fn}", Path.GetFileNameWithoutExtension(path));

        var result = PathUtils.GetValidFileName(newFileName);
        _logger.LogDebug("电影名: {Name} 生成结果: {Result}", movieMetadata.Name, result);
        return result;
    }

    public string GetMovieFolderName(MovieMetadata movieMetadata, string? pattern = null)
    {
        pattern ??= MOVIE_FOLDER_PATTERN;
        _logger.LogDebug("生成电影文件名 模板: {Pattern}, 电影:{Name}, 年份={Year}",
            pattern, movieMetadata.Name, movieMetadata.AirDate?.Year);

        var newFileName = pattern
            .Replace("{name}", GetValidName(movieMetadata.Name))
            .Replace("{oname}", GetValidName(movieMetadata.OriginalName))
            .Replace("{year}", movieMetadata.AirDate?.Year.ToString() ?? "Unknown");


        var result = PathUtils.GetValidFileName(newFileName);
        _logger.LogDebug("电影名: {Name} 生成结果: {Result}", movieMetadata.Name, result);
        return result;
    }

    private string GetValidName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        return PathUtils.GetValidFileName(name, ' ');
    }

    public FileNameGenerator(ILogger<FileNameGenerator> logger)
    {
        _logger = logger;
    }
}