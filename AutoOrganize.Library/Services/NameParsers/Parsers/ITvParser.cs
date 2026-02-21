using AutoOrganize.Library.Models;

namespace AutoOrganize.Library.Services.NameParsers.Parsers;

public interface ITvParser
{
    TvParseResult Parse(string filePath);
}