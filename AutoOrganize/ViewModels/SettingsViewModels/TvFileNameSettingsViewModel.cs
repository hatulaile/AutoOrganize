using AutoOrganize.Library.Services.PathNameGenerators.Configs;
using AutoOrganize.ViewModels.Abstractions;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.SettingsViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public class TvFileNameSettingsViewModel : ViewModelBase, INavigationViewModel
{
    public TvFileNameGenerationConfig NewConfig { get; internal set; } = new();
}