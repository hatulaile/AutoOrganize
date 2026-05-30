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

    public string GetTvSeriesFileName(SeriesMetadata seriesMetadata, ReadOnlySpan<char> pattern = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("剧集: \n模板: {Pattern} \n{@metadata}", pattern.ToString(), seriesMetadata);
        }

        if (pattern.IsEmpty)
            pattern = TV_SERIES_PATTERN;

        var name = seriesMetadata.Name.AsSpan();
        var originalName = seriesMetadata.OriginalName.AsSpan();

        int length = pattern.Length + name.Length + originalName.Length + 16;
        int pos = 0;
        Span<char> span = stackalloc char[length];
        int patternIndex = 0;
        while (patternIndex < pattern.Length)
        {
            int startIndex = pattern[patternIndex..].IndexOf('{');
            if (startIndex == -1)
            {
                pattern[patternIndex..].CopyTo(span[pos..]);
                pos += pattern.Length - patternIndex;
                break;
            }

            startIndex += patternIndex;

            int endIndex = pattern[(startIndex + 1)..].IndexOf('}');
            if (endIndex == -1)
            {
                pattern[patternIndex..].CopyTo(span[pos..]);
                pos += pattern.Length - patternIndex;
                break;
            }

            endIndex += startIndex + 1;

            pattern[patternIndex..startIndex].CopyTo(span[pos..]);
            pos += startIndex - patternIndex;
            patternIndex = endIndex + 1;

            var sourceToken = pattern[(startIndex + 1)..endIndex];
            if (IsToken(sourceToken, "sn"))
            {
                if (TryGetValidName(name, span[pos..], out int written))
                    pos += written;
            }
            else if (IsToken(sourceToken, "son"))
            {
                if (TryGetValidName(originalName, span[pos..], out int written))
                    pos += written;
            }
            else if (IsToken(sourceToken, "year"))
            {
                if (seriesMetadata.AirDate is { } date && date.Year.TryFormat(span[pos..], out int count))
                    pos += count;
            }
        }

        string result = span[..pos].ToString();
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("剧集: \n生成结果: {Result} \n{@metadata}", result, seriesMetadata);
        }

        return result;
    }

    public string GetTvSeasonFileName(SeasonMetadata seasonMetadata, ReadOnlySpan<char> pattern = default)
    {
        if (pattern.IsEmpty)
            pattern = TV_SEASON_PATTERN;

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("剧集季: \n模板: {Pattern} \n{@metadata}", pattern.ToString(), seasonMetadata);
        }

        if (seasonMetadata.SeasonNumber is not { } seasonNumber)
            throw new MetadataFieldNullException(nameof(SeasonMetadata), nameof(seasonMetadata.SeasonNumber));

        var name = seasonMetadata.Name.AsSpan();
        ReadOnlySpan<char> seriesName = seasonMetadata.Series?.Name is null ? [] : seasonMetadata.Series.Name.AsSpan();
        ReadOnlySpan<char> seriesOriginalName =
            seasonMetadata.Series?.OriginalName is null ? [] : seasonMetadata.Series.OriginalName.AsSpan();

        int length = pattern.Length + name.Length +
                     seriesName.Length + seriesOriginalName.Length + 16;

        int pos = 0;
        Span<char> span = stackalloc char[length];
        int patternIndex = 0;
        while (patternIndex < pattern.Length)
        {
            int startIndex = pattern[patternIndex..].IndexOf('{');
            if (startIndex == -1)
            {
                pattern[patternIndex..].CopyTo(span[pos..]);
                pos += pattern.Length - patternIndex;
                break;
            }

            startIndex += patternIndex;

            int endIndex = pattern[(startIndex + 1)..].IndexOf('}');
            if (endIndex == -1)
            {
                pattern[patternIndex..].CopyTo(span[pos..]);
                pos += pattern.Length - patternIndex;
                break;
            }

            endIndex += startIndex + 1;

            pattern[patternIndex..startIndex].CopyTo(span[pos..]);
            pos += startIndex - patternIndex;
            patternIndex = endIndex + 1;

            var sourceToken = pattern[(startIndex + 1)..endIndex];
            if (IsToken(sourceToken, "sn"))
            {
                if (!seriesName.IsEmpty && TryGetValidName(seriesName, span[pos..], out int written))
                    pos += written;
            }
            else if (IsToken(sourceToken, "son"))
            {
                if (!seriesOriginalName.IsEmpty && TryGetValidName(seriesOriginalName, span[pos..], out int written))
                    pos += written;
            }
            else if (IsToken(sourceToken, "snn"))
            {
                if (TryGetValidName(name, span[pos..], out int written))
                    pos += written;
            }
            else if (IsToken(sourceToken, "year") && seasonMetadata.AirDate is { } date)
            {
                if (date.Year.TryFormat(span[pos..], out int count))
                    pos += count;
            }
            else if (TryGetZeroPaddedCount(sourceToken, "s", out int count))
            {
                if (seasonNumber.TryFormat(span[pos..], out int numCount, $"D{count}"))
                    pos += numCount;
            }
        }

        var result = span[..pos].ToString();
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("剧集季: \n结果: {Result} \n{@metadata}", result, seasonMetadata);
        return result;
    }

    public string GetTvEpisodeFileName(string path, EpisodeMetadata episodeMetadata,
        ReadOnlySpan<char> pattern = default)
    {
        if (pattern.IsEmpty)
            pattern = TV_EPISODE_PATTERN;

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("剧集集: \n模板: {Pattern} \n{@metadata}", pattern.ToString(), episodeMetadata);
        }

        if (episodeMetadata.EpisodeNumber is not { } seasonNumber)
            throw new MetadataFieldNullException(nameof(EpisodeMetadata), nameof(episodeMetadata.SeasonNumber));

        if (episodeMetadata.EpisodeNumber is not { } episodeNumber)
            throw new MetadataFieldNullException(nameof(EpisodeMetadata), nameof(episodeMetadata.EpisodeNumber));

        ReadOnlySpan<char> seriesName =
            episodeMetadata.Series?.Name is null ? [] : episodeMetadata.Series.Name.AsSpan();
        ReadOnlySpan<char> seriesOriginalName =
            episodeMetadata.Series?.OriginalName is null ? [] : episodeMetadata.Series.OriginalName.AsSpan();
        ReadOnlySpan<char> seasonName =
            episodeMetadata.Season?.Name is null ? [] : episodeMetadata.Season.Name.AsSpan();
        ReadOnlySpan<char> name = episodeMetadata.Name;
        ReadOnlySpan<char> ext = Path.GetExtension(path).TrimStart('.');
        ReadOnlySpan<char> fileName = Path.GetFileNameWithoutExtension(path);


        int length = path.Length + seriesName.Length + seasonName.Length +
                     name.Length + ext.Length + fileName.Length + 16;

        int pos = 0;
        Span<char> span = stackalloc char[length];
        int patternIndex = 0;
        while (patternIndex < pattern.Length)
        {
            int startIndex = pattern[patternIndex..].IndexOf('{');
            if (startIndex == -1)
            {
                pattern[patternIndex..].CopyTo(span[pos..]);
                pos += pattern.Length - patternIndex;
                break;
            }

            startIndex += patternIndex;

            int endIndex = pattern[(startIndex + 1)..].IndexOf('}');
            if (endIndex == -1)
            {
                pattern[patternIndex..].CopyTo(span[pos..]);
                pos += pattern.Length - patternIndex;
                break;
            }

            endIndex += startIndex + 1;

            pattern[patternIndex..startIndex].CopyTo(span[pos..]);
            pos += startIndex - patternIndex;
            patternIndex = endIndex + 1;

            var sourceToken = pattern[(startIndex + 1)..endIndex];
            if (IsToken(sourceToken, "sn"))
            {
                if (!seriesName.IsEmpty && TryGetValidName(seriesName, span[pos..], out int written))
                    pos += written;
            }
            else if (IsToken(sourceToken, "son"))
            {
                if (!seriesOriginalName.IsEmpty && TryGetValidName(seriesOriginalName, span[pos..], out int written))
                    pos += written;
            }
            else if (IsToken(sourceToken, "snn"))
            {
                if (TryGetValidName(seasonName, span[pos..], out int written))
                    pos += written;
            }
            else if (IsToken(sourceToken, "en"))
            {
                if (TryGetValidName(name, span[pos..], out int written))
                    pos += written;
            }
            else if (IsToken(sourceToken, "fn"))
            {
                if (TryGetValidName(fileName, span[pos..], out int written))
                    pos += written;
            }
            else if (IsToken(sourceToken, "ext"))
            {
                if (TryGetValidName(ext, span[pos..], out int written))
                    pos += written;
            }
            else if (IsToken(sourceToken, "year") && episodeMetadata.AirDate is { } date)
            {
                if (date.Year.TryFormat(span[pos..], out int count))
                    pos += count;
            }
            else if (TryGetZeroPaddedCount(sourceToken, "s", out int count))
            {
                if (seasonNumber.TryFormat(span[pos..], out int numCount, $"D{count}"))
                    pos += numCount;
            }
            else if (TryGetZeroPaddedCount(sourceToken, "e", out count))
            {
                if (episodeNumber.TryFormat(span[pos..], out int numCount, $"D{count}"))
                    pos += numCount;
            }
        }

        var result = span[..pos].ToString();
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("剧集集 \n结果: {Result} \n{@metadata}", result, episodeMetadata);
        }

        return result;
    }

    public string GetMovieFileName(string path, MovieMetadata movieMetadata, ReadOnlySpan<char> pattern = default)
    {
        if (pattern.IsEmpty)
            pattern = MOVIE_PATTERN;

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("生成电影文件名 模板: {Pattern}, 电影:{Name}, 年份={Year}",
                pattern.ToString(), movieMetadata.Name, movieMetadata.AirDate?.Year);
        }

        ReadOnlySpan<char> name = movieMetadata.Name is null ? [] : movieMetadata.Name.AsSpan();
        ReadOnlySpan<char> originalName = movieMetadata.OriginalName is null ? [] : movieMetadata.OriginalName.AsSpan();
        ReadOnlySpan<char> fileName = Path.GetFileNameWithoutExtension(path);
        ReadOnlySpan<char> ext = Path.GetExtension(path).TrimStart('.');

        int length = path.Length + name.Length + originalName.Length + fileName.Length + ext.Length + 16;
        int pos = 0;
        Span<char> span = stackalloc char[length];
        int patternIndex = 0;
        while (patternIndex < pattern.Length)
        {
            int startIndex = pattern[patternIndex..].IndexOf('{');
            if (startIndex == -1)
            {
                pattern[patternIndex..].CopyTo(span[pos..]);
                pos += pattern.Length - patternIndex;
                break;
            }

            startIndex += patternIndex;

            int endIndex = pattern[(startIndex + 1)..].IndexOf('}');
            if (endIndex == -1)
            {
                pattern[patternIndex..].CopyTo(span[pos..]);
                pos += pattern.Length - patternIndex;
                break;
            }

            endIndex += startIndex + 1;

            pattern[patternIndex..startIndex].CopyTo(span[pos..]);
            pos += startIndex - patternIndex;
            patternIndex = endIndex + 1;

            var sourceToken = pattern[(startIndex + 1)..endIndex];
            if (IsToken(sourceToken, "name"))
            {
                if (TryGetValidName(name, span[pos..], out int written))
                    pos += written;
            }
            else if (IsToken(sourceToken, "oname"))
            {
                if (TryGetValidName(originalName, span[pos..], out int written))
                    pos += written;
            }
            else if (IsToken(sourceToken, "year"))
            {
                if (movieMetadata.AirDate is { } date && date.Year.TryFormat(span[pos..], out int written))
                    pos += written;
            }
            else if (IsToken(sourceToken, "fn"))
            {
                if (TryGetValidName(fileName, span[pos..], out int written))
                    pos += written;
            }
            else if (IsToken(sourceToken, "ext"))
            {
                if (TryGetValidName(ext, span[pos..], out int written))
                    pos += written;
            }
        }


        string result = span[..pos].ToString();
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("电影: \n 生成结果: {Result} \n{@metadata}", result, movieMetadata);
        }

        return result;
    }

    public string GetMovieFolderName(MovieMetadata movieMetadata, ReadOnlySpan<char> pattern = default)
    {
        if (pattern.IsEmpty)
            pattern = MOVIE_FOLDER_PATTERN;

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("电影: \n模板: {Pattern} \n{@metadata}", pattern.ToString(), movieMetadata);
        }

        ReadOnlySpan<char> name = movieMetadata.Name is null ? [] : movieMetadata.Name;
        ReadOnlySpan<char> originalName = movieMetadata.OriginalName is null ? [] : movieMetadata.OriginalName;


        int length = pattern.Length + name.Length + originalName.Length;
        int pos = 0;
        Span<char> span = stackalloc char[length];
        int patternIndex = 0;
        while (patternIndex < pattern.Length)
        {
            int startIndex = pattern[patternIndex..].IndexOf('{');
            if (startIndex == -1)
            {
                pattern[patternIndex..].CopyTo(span[pos..]);
                pos += pattern.Length - patternIndex;
                break;
            }

            startIndex += patternIndex;

            int endIndex = pattern[(startIndex + 1)..].IndexOf('}');
            if (endIndex == -1)
            {
                pattern[patternIndex..].CopyTo(span[pos..]);
                pos += pattern.Length - patternIndex;
                break;
            }

            endIndex += startIndex + 1;

            pattern[patternIndex..startIndex].CopyTo(span[pos..]);
            pos += startIndex - patternIndex;
            patternIndex = endIndex + 1;

            var sourceToken = pattern[(startIndex + 1)..endIndex];
            if (IsToken(sourceToken, "name"))
            {
                if (TryGetValidName(name, span[pos..], out int written))
                    pos += written;
            }
            else if (IsToken(sourceToken, "oname"))
            {
                if (TryGetValidName(originalName, span[pos..], out int written))
                    pos += written;
            }
            else if (IsToken(sourceToken, "year"))
            {
                if (movieMetadata.AirDate is { } date && date.Year.TryFormat(span[pos..], out int count))
                    pos += count;
            }
        }


        var result = PathUtils.GetValidFileName(span[..pos].ToString());
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("电影文件夹: \n生成: {Result} \n{@metadata}", result, movieMetadata);
        }

        return result;
    }

    private bool TryGetValidName(ReadOnlySpan<char> source, Span<char> target, out int written)
    {
        return PathUtils.TryGetValidFileName(source, target, out written);
    }

    private static bool TryGetZeroPaddedCount(in ReadOnlySpan<char> source, in ReadOnlySpan<char> token, out int count)
    {
        if (!HasToken(source, token))
        {
            count = 0;
            return false;
        }

        count = source.Length - token.Length;
        return true;
    }

    private static bool HasToken(in ReadOnlySpan<char> source, in ReadOnlySpan<char> token)
    {
        if (token.Length > source.Length)
            return false;

        for (int i = 0; i < token.Length; i++)
        {
            if (source[i] == token[i])
                continue;

            //由于这里 token 肯定是 lower 所以不用 tolower
            if (char.ToLowerInvariant(source[i]) == token[i])
                continue;

            return false;
        }

        return true;
    }

    private static bool IsToken(in ReadOnlySpan<char> source, in ReadOnlySpan<char> token)
    {
        if (source.Length != token.Length)
            return false;

        for (int i = 0; i < source.Length; i++)
        {
            if (source[i] == token[i])
                continue;

            //由于这里 token 肯定是 lower 所以不用 tolower
            if (char.ToLowerInvariant(source[i]) == token[i])
                continue;

            return false;
        }

        return true;
    }

    public FileNameGenerator(ILogger<FileNameGenerator> logger)
    {
        _logger = logger;
    }
}