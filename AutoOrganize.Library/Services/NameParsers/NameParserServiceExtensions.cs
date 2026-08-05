using AutoOrganize.Library.Models;

namespace AutoOrganize.Library.Services.NameParsers;

public static class NameParserServiceExtensions
{
    public static TvParseResult ParseTv(this INameParserService nameParser, string filePath) =>
        nameParser.Parse<TvParseResult>(filePath);

    public static MovieParseResult ParseMovie(this INameParserService nameParser, string filePath) =>
        nameParser.Parse<MovieParseResult>(filePath);
}