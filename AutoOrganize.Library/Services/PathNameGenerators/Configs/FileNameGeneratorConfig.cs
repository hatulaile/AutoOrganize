using AutoConfigGenerator;
using AutoOrganize.Library.Services.Config;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoOrganize.Library.Services.PathNameGenerators.Configs;

[AutoConfig]
public sealed partial class FileNameGeneratorConfig : ConfigBase<FileNameGeneratorConfig>
{
    [ObservableProperty]
    public partial TvFileNameGenerationConfig TvFileNameGenerationConfig { get; set; } = new();

    [ObservableProperty]
    public partial MovieFileNameGeneratorConfig MovieFileNameGeneratorConfig { get; set; } = new();
}