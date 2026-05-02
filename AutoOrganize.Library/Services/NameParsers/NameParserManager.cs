using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.NameParsers.Parsers;
using Microsoft.Extensions.Logging;

namespace AutoOrganize.Library.Services.NameParsers;

public sealed class NameParserManager : INameParserManager
{
    private readonly IEnumerable<ITvParser> _tvNameParsers;
    private readonly IEnumerable<IMovieParser> _movieNameParsers;
    private readonly ILogger<NameParserManager> _logger;

    public TvParseResult ParseTv(string filePath)
    {
        _logger.LogDebug("开始解析电视剧文件名: {FilePath}", filePath);
        var result = new TvParseResult();
        foreach (ITvParser parser in _tvNameParsers)
        {
            var partial = parser.Parse(filePath);
            _logger.LogDebug("使用解析器 {ParserType} 解析电视剧: {@Result}", parser.GetType().Name, partial);
            result.Complement(partial);
            if (result.IsComplete(true))
            {
                _logger.LogDebug("电视剧解析完成: {FilePath} -> {Title} (S{Season:D2}E{Episode:D2})",
                    filePath, result.Title, result.Season ?? 0, result.Episode ?? 0);
                break;
            }
        }

        return result;
    }

    public MovieParseResult ParseMovie(string filePath)
    {
        _logger.LogDebug("开始解析电影文件名: {FilePath}", filePath);
        var result = new MovieParseResult();
        foreach (IMovieParser parser in _movieNameParsers)
        {
            var partial = parser.Parse(filePath);
            _logger.LogDebug("使用解析器 {ParserType} 解析电影: {@Result}", parser.GetType().Name, partial);
            result.Complement(partial);
            if (result.IsComplete(true))
            {
                _logger.LogDebug("电影解析完成: {FilePath} -> {Title} ({Year})",
                    filePath, result.Title, result.Year);
                break;
            }
        }

        return result;
    }

    public NameParserManager(IEnumerable<ITvParser> tvNameParsers, IEnumerable<IMovieParser> movieNameParsers,
        ILogger<NameParserManager> logger)
    {
        _tvNameParsers = tvNameParsers;
        _movieNameParsers = movieNameParsers;
        _logger = logger;
    }
}