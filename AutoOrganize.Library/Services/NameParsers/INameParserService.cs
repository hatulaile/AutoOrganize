using AutoOrganize.Library.Models;

namespace AutoOrganize.Library.Services.NameParsers;

public interface INameParserService
{
    TResult Parse<TResult>(string filePath) where TResult : class, IParseResult<TResult>, new();
}