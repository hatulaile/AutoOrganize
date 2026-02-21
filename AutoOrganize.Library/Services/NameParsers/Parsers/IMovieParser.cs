using AutoOrganize.Library.Models;

namespace AutoOrganize.Library.Services.NameParsers.Parsers;

public interface IMovieParser
{
    MovieParseResult Parse(string filePath);
}