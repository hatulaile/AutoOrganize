using AutoConfigGenerator;
using AutoOrganize.Library.Services.Config;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoOrganize.Library.Services.PathNameGenerators.Configs;

[AutoConfig]
public partial class MovieFileNameGeneratorConfig : ConfigBase<MovieFileNameGeneratorConfig>
{
    [ObservableProperty]
    public partial string MoviePattern { get; set; } = FileNameGenerator.MOVIE_PATTERN;

    [ObservableProperty]
    public partial string MovieFolderPattern { get; set; } = FileNameGenerator.MOVIE_FOLDER_PATTERN;
}