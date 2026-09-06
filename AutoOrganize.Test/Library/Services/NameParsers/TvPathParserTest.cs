using AutoOrganize.Library.Models;
using AutoOrganize.Library.Services.NameParsers.Parsers;

namespace AutoOrganize.Test.Library.Services.NameParsers;

public class TvPathParserTest
{
    private readonly TvPathParser _parser = new(new ParserOptions());

    [Fact]
    public void Parse_WithTrailingSeparatedNumber_ParsesEpisode()
    {
        TvParseResult result = _parser.Parse("Sono Bisque Doll wa Koi wo Suru Season 2 - 21.mkv");

        Assert.Equal("Sono Bisque Doll wa Koi wo Suru", result.Title);
        Assert.Equal(2, result.Season);
        Assert.Equal(21, result.Episode);
    }

    [Fact]
    public void Parse_WithTrailingYear_DoesNotParseYearAsEpisode()
    {
        TvParseResult result = _parser.Parse("Sono Bisque Doll wa Koi wo Suru Season 2 - 2024.mkv");

        Assert.Null(result.Episode);
        Assert.Equal(2024, result.Year);
    }
}