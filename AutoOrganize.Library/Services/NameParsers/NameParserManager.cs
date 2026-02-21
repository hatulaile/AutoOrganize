using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.NameParsers.Parsers;

namespace AutoOrganize.Library.Services.NameParsers;

public sealed class NameParserManager : INameParserManager
{
    private readonly IEnumerable<ITvParser> _tvNameParsers;
    private readonly IEnumerable<IMovieParser> _movieNameParsers;

    public TvParseResult ParseTv(string filePath)
    {
        var result = new TvParseResult();
        foreach (ITvParser parser in _tvNameParsers)
        {
            result.Complement(parser.Parse(filePath));
            if (result.IsComplete(true))
                break;
        }

        return result;
    }

    public MovieParseResult ParseMovie(string filePath)
    {
        var result = new MovieParseResult();
        foreach (IMovieParser parser in _movieNameParsers)
        {
            result.Complement(parser.Parse(filePath));
            if (result.IsComplete(true))
                break;
        }

        return result;
    }

    public NameParserManager(IEnumerable<ITvParser> tvNameParsers, IEnumerable<IMovieParser> movieNameParsers)
    {
        _tvNameParsers = tvNameParsers;
        _movieNameParsers = movieNameParsers;
    }
}