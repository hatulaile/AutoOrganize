using AutoOrganize.Library.Services.Config;
using AutoOrganize.Library.Services.Metadata.Providers.ThemoviedbMetadataProviders;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.SettingsViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public class ThemoviedbMetadataProviderSettingsViewModel : SettingsViewModelBase<ThemoviedbMetadataProviderConfig>
{
    public ThemoviedbMetadataProviderSettingsViewModel(IFileConfigManager configManager) : base(configManager)
    {
    }
}