namespace AutoOrganize.Library.Models;

public sealed class MovieParseResult : IParseResult<MovieParseResult>
{
    public string? Title { get; set; }

    public int? Year { get; set; }

    public void Complement(MovieParseResult other)
    {
        Title ??= other.Title;
        Year ??= other.Year;
    }
}