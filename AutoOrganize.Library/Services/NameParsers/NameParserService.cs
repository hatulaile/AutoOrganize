using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.NameParsers.Parsers;
using Microsoft.Extensions.Logging;

namespace AutoOrganize.Library.Services.NameParsers;

public sealed class NameParserService : INameParserService
{
    private readonly IEnumerable<INameParserStrategy> _strategies;
    private readonly ILogger<NameParserService> _logger;

    public TResult Parse<TResult>(string filePath) where TResult : class, IParseResult<TResult>, new()
    {
        _logger.LogDebug("开始解析文件名: {FilePath}", filePath);
        var result = new TResult();

        foreach (INameParserStrategy<TResult> strategy in _strategies.OfType<INameParserStrategy<TResult>>())
        {
            TResult partial = strategy.Parse(filePath);
            _logger.LogDebug("解析策略 {ParserType} 产生结果: {@Result}", strategy.GetType().Name, partial);
            result.Complement(partial);
        }

        return result;
    }

    public NameParserService(IEnumerable<INameParserStrategy> strategies, ILogger<NameParserService> logger)
    {
        _strategies = strategies;
        _logger = logger;
    }
}