using AutoOrganize.Library.Models;

namespace AutoOrganize.Library.Services.NameParsers;

public interface INameParserManager
{
    TvParseResult ParseTv(string filePath);
    MovieParseResult ParseMovie(string filePath);
}