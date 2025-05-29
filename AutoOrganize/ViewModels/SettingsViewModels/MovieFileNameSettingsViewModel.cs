using AutoOrganize.Library.Services.PathNameGenerators;
using AutoOrganize.Library.Services.PathNameGenerators.Configs;
using AutoOrganize.Services.NavigationServices;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.SettingsViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public class MovieFileNameSettingsViewModel : ViewModelBase, INavigationViewModel
{
    private readonly IFileNameGenerator _fileNameGenerator;

    public MovieFileNameGeneratorConfig NewConfig { get; internal set; } = new();

    public MovieFileNameSettingsViewModel(IFileNameGenerator fileNameGenerator)
    {
        _fileNameGenerator = fileNameGenerator;
    }
}