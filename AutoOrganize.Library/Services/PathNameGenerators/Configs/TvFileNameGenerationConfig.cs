namespace AutoOrganize.Library.Services.PathNameGenerators.Configs;

public class TvFileNameGenerationConfig
{
    public string SeriesMetadataFolderPattern { get; set; } = PathNameGenerator.TV_SERIES_PATTERN;

    public string SeasonMetadataFolderPattern { get; set; } = PathNameGenerator.TV_SEASON_PATTERN;

    public string EpisodeNamePattern { get; set; } = PathNameGenerator.TV_EPISODE_PATTERN;
}