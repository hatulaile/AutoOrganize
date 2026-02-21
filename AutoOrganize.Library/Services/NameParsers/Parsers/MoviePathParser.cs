using System.Text.RegularExpressions;
using AutoOrganize.Library.Models;

namespace AutoOrganize.Library.Services.NameParsers.Parsers;

public class MoviePathParser : IMovieParser
{
    private readonly ParserOptions _options;

    public MovieParseResult Parse(string filePath)
    {
        var info = new FileInfo(filePath);
        var result = new MovieParseResult();
        result.Complement(ParseInternal(info.Name, true));

        DirectoryInfo? parent = info.Directory;
        for (int i = 0; i < _options.MaxNestingLevel; i++)
        {
            if (parent is null || result.IsComplete(true)) break;
            result.Complement(ParseInternal(parent.Name, false));
            parent = parent.Parent;
        }

        return result;
    }

    private MovieParseResult ParseInternal(string directoryPath, bool isFile)
    {
        var result = new MovieParseResult();

        foreach (Regex seasonRegex in _options.YearRegexes)
        {
            var match = seasonRegex.Matches(directoryPath).LastOrDefault();
            if (match is null)
                continue;
            if (!int.TryParse(match.Groups["year"].Value, out int i))
                continue;

            result.Year = i;
            directoryPath = directoryPath.Remove(match.Index, match.Length);
            break;
        }

        if (isFile) directoryPath = Path.GetFileNameWithoutExtension(directoryPath);
        result.Title = CleanTitle(directoryPath);
        return result;
    }

    private string CleanTitle(string title)
    {
        title = _options.CleanTitleRegex.Aggregate(title, (current, regex) => regex.Replace(current, string.Empty));

        title = title
            .Replace('.', ' ').Replace('-', ' ').Replace('_', ' ')
            .Replace('[', ' ').Replace(']', ' ')
            .Replace('(', ' ').Replace(')', ' ').Trim();
        int index = title.IndexOf("  ", StringComparison.Ordinal);
        if (index >= 0) title = title.Remove(index).Trim();
        return title;
    }

    public MoviePathParser(ParserOptions options)
    {
        _options = options;
    }
}