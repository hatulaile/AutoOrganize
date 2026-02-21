// ReSharper disable CheckNamespace

using System.Text.RegularExpressions;

namespace AutoOrganize.Library.Models;

public sealed partial class ParserOptions
{
    // 匹配形式: S01 / S1
    [GeneratedRegex(@"\bS(?<season>\d{1,2})(?=E|\b|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex SeasonRegex1 { get; }

    // 匹形式：Season 3 / Season3（英文单词 Season + 数字）
    [GeneratedRegex(@"\bSeason\s*(?<season>\d{1,2})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex SeasonRegex2 { get; }

    // 匹形式：第3季
    [GeneratedRegex(@"第(?<season>\d{1,2})季", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    public static partial Regex SeasonRegex3 { get; }

    // 匹形式：[S02]
    [GeneratedRegex(@"\[S(?<season>\d{1,2})\]", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    public static partial Regex SeasonRegex4 { get; }

    // 匹配形式: E01 / E1
    [GeneratedRegex(@"\bE(?<episode>\d{1,3})(?=\b|$|\D)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex EpisodeRegex1 { get; }

    // 匹配形式：Episode 01 / Episode 1
    [GeneratedRegex(@"\bEpisode\s*(?<episode>\d{1,3})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex EpisodeRegex2 { get; }

    // 匹配形式：第01话 / 第1話 / 第01集
    [GeneratedRegex(@"第(?<episode>\d{1,3})[话話集]", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    public static partial Regex EpisodeRegex3 { get; }

    // 匹配形式：方括号 [01] / [1]
    [GeneratedRegex(@"\[(?<episode>\d{1,3})\]", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    public static partial Regex EpisodeRegex4 { get; }

    [GeneratedRegex(@"(?<!\d)(?<year>(?:19|20)\d{2})(?!\d|×|x)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex YearRegex { get; }

    [GeneratedRegex(@"\[.*?\]")]
    public static partial Regex CleanRegex1 { get; }

    [GeneratedRegex(@"\(.*?\)")]
    public static partial Regex CleanRegex2 { get; }

    [GeneratedRegex(@"（.*?\）")]
    public static partial Regex CleanRegex3 { get; }
}