using System.Diagnostics.CodeAnalysis;

namespace AutoOrganize.Library.Models;

public sealed class TvParseResult
{
    public string? Title { get; set; }

    public int? Year { get; set; }

    public int? Season { get; set; }

    public long? Episode { get; set; }

    public void Complement(TvParseResult other)
    {
        if (other.Title is not null)
            Title ??= other.Title;

        if (other.Year is not null)
            Year ??= other.Year;

        if (other.Season is not null)
            Season ??= other.Season;

        if (other.Episode is not null)
            Episode = other.Episode;
    }

    [MemberNotNullWhen(true, nameof(Title), nameof(Season), nameof(Episode))]
    public bool IsComplete(bool hasExtension = false)
    {
        if (!hasExtension)
            return Title is not null && Season is not null && Episode is not null;
        return Title is not null && Year is not null && Season is not null && Episode is not null;
    }
}