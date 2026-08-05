namespace AutoOrganize.Library.Models;

public sealed class TvParseResult : IParseResult<TvParseResult>
{
    public string? Title { get; set; }

    public int? Year { get; set; }

    public int? Season { get; set; }

    public long? Episode { get; set; }

    public void Complement(TvParseResult other)
    {
        Title ??= other.Title;
        Year ??= other.Year;
        Season ??= other.Season;
        Episode ??= other.Episode;
    }

    public bool IsComplete()
    {
        return Title is not null && Year is not null && Season is not null && Episode is not null;
    }
}