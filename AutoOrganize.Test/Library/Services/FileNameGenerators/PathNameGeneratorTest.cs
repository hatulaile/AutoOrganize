using AutoOrganize.Library.Services.PathNameGenerators;
using AutoOrganize.Library.Services.PathNameGenerators.Options;
using AutoOrganize.Test.Utils;

namespace AutoOrganize.Test.Library.Services.FileNameGenerators;

public class PathNameGeneratorTest
{
    private readonly PathNameGenerator _generator;

    public PathNameGeneratorTest()
    {
        _generator = new PathNameGenerator();
    }

    [Fact]
    public void GetTvSeriesFileName_WithCustomPatternContainingSnSonAndYear_ReturnsFormattedString()
    {
        var metadata = MetadataUtils.CreateSeriesMetadataExample();

        string name = _generator.GetTvSeriesFileName(metadata, new TvFileNameGenerationOptions
        {
            SeriesMetadataFolderPattern = "{sn}.{son}.{year}"
        });

        Assert.Equal("轻音少女.けいおん!.2009", name);
    }

    [Fact]
    public void GetTvSeriesFileName_WithDefaultOptions_ReturnsSeriesNameAndYearOnly()
    {
        var metadata = MetadataUtils.CreateSeriesMetadataExample();

        string name = _generator.GetTvSeriesFileName(metadata);

        Assert.Equal("轻音少女.2009", name);
    }

    [Fact]
    public void GetTvSeasonFileName_WithCustomPatternContainingMultipleFields_ReturnsFormattedString()
    {
        var metadata = MetadataUtils.CreateSeasonMetadataExample();

        string name = _generator.GetTvSeasonFileName(metadata, new TvFileNameGenerationOptions
        {
            SeasonMetadataFolderPattern = "{sn}.{son}.{snn}.{year}.{s0}"
        });

        Assert.Equal("轻音少女.けいおん!.轻音少女！.2009.1", name);
    }

    [Fact]
    public void GetTvSeasonFileName_WithDefaultOptions_ReturnsSeasonWithTwoDigitPadding()
    {
        var metadata = MetadataUtils.CreateSeasonMetadataExample();

        string name = _generator.GetTvSeasonFileName(metadata);

        Assert.Equal("Season 01", name);
    }

    [Theory]
    [InlineData("{s0}", "1")]
    [InlineData("{s00}", "01")]
    [InlineData("{s000}", "001")]
    [InlineData("{s0000}", "0001")]
    public void GetTvSeasonFileName_WithSeasonNumberPaddingPattern_ReturnsCorrectlyPaddedNumber(string input,
        string expected)
    {
        var metadata = MetadataUtils.CreateSeasonMetadataExample();
        string name = _generator.GetTvSeasonFileName(metadata, new TvFileNameGenerationOptions
        {
            SeasonMetadataFolderPattern = input
        });
        Assert.Equal(expected, name);
    }

    [Fact]
    public void GetTvEpisodeFileName_WithCustomPatternContainingAllFields_ReturnsFormattedString()
    {
        var metadata = MetadataUtils.CreateEpisodeMetadataExample();

        string name = _generator.GetTvEpisodeFileName("1.mkv", metadata, new TvFileNameGenerationOptions
        {
            EpisodeNamePattern = "{sn}.{son}.{snn}.{en}.{s0}.{e0}.{year}.{fn}.{ext}"
        });

        Assert.Equal("轻音少女.けいおん!.轻音少女！.废部！.1.1.2009.1.mkv", name);
    }

    [Fact]
    public void GetTvEpisodeFileName_WithDefaultOptions_ReturnsStandardNamingWithSeasonAndEpisode()
    {
        var metadata = MetadataUtils.CreateEpisodeMetadataExample();

        string name = _generator.GetTvEpisodeFileName("1.mkv", metadata);

        Assert.Equal("轻音少女.S01E01 - 废部！.mkv", name);
    }

    [Theory]
    [InlineData("{s0}{e0}", "11")]
    [InlineData("{s00}{e00}", "0101")]
    [InlineData("{s000}{e000}", "001001")]
    [InlineData("{s0000}{e0000}", "00010001")]
    public void GetTvEpisodeFileName_WithSeasonAndEpisodeNumberPaddingPattern_ReturnsConcatenatedNumbers(string input,
        string expected)
    {
        var metadata = MetadataUtils.CreateEpisodeMetadataExample();
        string name = _generator.GetTvEpisodeFileName(string.Empty, metadata, new TvFileNameGenerationOptions
        {
            EpisodeNamePattern = input
        });
        Assert.Equal(expected, name);
    }

    [Fact]
    public void GetMovieFolderName_WithCustomPatternContainingNameOnameAndYear_ReturnsFormattedString()
    {
        var metadata = MetadataUtils.CreateMovieMetadataExample();

        string name = _generator.GetMovieFolderName(metadata, new MovieFileNameGenerationOptions
        {
            MovieFolderPattern = "{name}.{oname}.{year}"
        });

        Assert.Equal("悠哉日常大王剧场版：假期活动.劇場版 のんのんびより ばけーしょん", name);
    }

    [Fact]
    public void GetMovieFolderName_WithDefaultOptions_ReturnsMovieNameAndYearOnly()
    {
        var metadata = MetadataUtils.CreateMovieMetadataExample();

        string name = _generator.GetMovieFolderName(metadata);

        Assert.Equal("悠哉日常大王剧场版：假期活动.2018", name);
    }

    [Fact]
    public void GetMovieFileName_WithCustomPatternContainingAllFields_ReturnsFormattedString()
    {
        var metadata = MetadataUtils.CreateMovieMetadataExample();

        string name = _generator.GetMovieFileName("1.mkv", metadata, new MovieFileNameGenerationOptions
        {
            MoviePattern = "{name}.{oname}.{year}.{fn}.{ext}"
        });

        Assert.Equal("悠哉日常大王剧场版：假期活动.劇場版 のんのんびより ばけーしょん.2018.1.mkv", name);
    }

    [Fact]
    public void GetMovieFileName_WithDefaultOptions_ReturnsMovieNameAndYearWithExtension()
    {
        var metadata = MetadataUtils.CreateMovieMetadataExample();
        string name = _generator.GetMovieFileName("悠哉日常大王剧场版：假期活动.mkv", metadata);
        Assert.Equal("悠哉日常大王剧场版：假期活动.2018.mkv", name);
    }
}