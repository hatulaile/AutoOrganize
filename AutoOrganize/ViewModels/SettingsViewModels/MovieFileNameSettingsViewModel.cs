using AutoOrganize.Library.Services.PathNameGenerators.Configs;
using AutoOrganize.ViewModels.Abstractions;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.SettingsViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public class MovieFileNameSettingsViewModel : ViewModelBase, INavigationViewModel
{
    public MovieFileNameGeneratorConfig NewConfig { get; internal set; } = new();
}