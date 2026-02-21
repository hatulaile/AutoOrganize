// ReSharper disable CheckNamespace

using System.Text.RegularExpressions;

namespace AutoOrganize.Library.Models;

public sealed partial class ParserOptions
{
    public Regex[] SeasonRegexes { get; } =
    [
        SeasonRegex1,
        SeasonRegex2,
        SeasonRegex3,
        SeasonRegex4,
    ];

    public Regex[] EpisodeRegexes { get; } =
    [
        EpisodeRegex1,
        EpisodeRegex2,
        EpisodeRegex3,
        EpisodeRegex4,
    ];

    public Regex[] YearRegexes { get; } =
    [
        YearRegex
    ];

    public Regex[] CleanTitleRegex { get; } =
    [
        CleanRegex1,
        CleanRegex2,
        CleanRegex3
    ];

    public ushort MaxNestingLevel { get; set; } = 5;
}