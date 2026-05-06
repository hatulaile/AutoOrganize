using AutoConfigGenerator;
using AutoOrganize.Library.Services.Config;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoOrganize.Library.Services.PathNameGenerators.Configs;

[AutoConfig]
public partial class TvFileNameGenerationConfig : ConfigBase<TvFileNameGenerationConfig>
{
    [ObservableProperty]
    public partial string SeriesMetadataFolderPattern { get; set; } = PathNameGenerator.TV_SERIES_PATTERN;

    [ObservableProperty]
    public partial string SeasonMetadataFolderPattern { get; set; } = PathNameGenerator.TV_SEASON_PATTERN;

    [ObservableProperty]
    public partial string EpisodeNamePattern { get; set; } = PathNameGenerator.TV_EPISODE_PATTERN;
}