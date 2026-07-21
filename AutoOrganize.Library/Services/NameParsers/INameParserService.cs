using AutoOrganize.Library.Models;

namespace AutoOrganize.Library.Services.NameParsers;

public interface INameParserService
{
    TvParseResult ParseTv(string filePath);
    MovieParseResult ParseMovie(string filePath);
}