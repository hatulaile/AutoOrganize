using AutoOrganize.Library.Services.PathNameGenerators;
using AutoOrganize.Library.Services.PathNameGenerators.Configs;
using AutoOrganize.ViewModels.Abstractions;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.SettingsViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public class TvFileNameSettingsViewModel : ViewModelBase, INavigationViewModel
{
    private readonly IFileNameGenerator _fileNameGenerator;

    public TvFileNameGenerationConfig NewConfig { get; internal set; } = new();

    public TvFileNameSettingsViewModel(IFileNameGenerator fileNameGenerator)
    {
        _fileNameGenerator = fileNameGenerator;
    }
}