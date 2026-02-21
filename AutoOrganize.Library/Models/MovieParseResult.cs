using System.Diagnostics.CodeAnalysis;

namespace AutoOrganize.Library.Models;

public sealed class MovieParseResult
{
    public string? Title { get; set; }

    public int? Year { get; set; }

    public void Complement(MovieParseResult other)
    {
        if (other.Title is not null)
            Title ??= other.Title;

        if (other.Year is not null)
            Year ??= other.Year;
    }

    [MemberNotNullWhen(true, nameof(Title))]
    public bool IsComplete(bool hasExtension = false)
    {
        if (!hasExtension)
            return Title is not null;
        return Title is not null && Year is not null;
    }
}