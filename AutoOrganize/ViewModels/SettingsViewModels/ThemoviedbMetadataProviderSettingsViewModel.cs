using AutoOrganize.Library.Services.Config;
using AutoOrganize.Library.Services.Metadata.Providers;
using ViewModelRegistrationGenerator;
using ThemoviedbProviderConfig = AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbProviders.ThemoviedbProviderConfig;

namespace AutoOrganize.ViewModels.SettingsViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public class ThemoviedbMetadataProviderSettingsViewModel
    : SettingsViewModelBase<ThemoviedbProviderConfig>
{
    public ThemoviedbMetadataProviderSettingsViewModel(IFileConfigManager configManager) : base(configManager)
    {
    }
}