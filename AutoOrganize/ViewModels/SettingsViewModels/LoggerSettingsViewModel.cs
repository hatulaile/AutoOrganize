using AutoOrganize.Library.Services.Config;
using AutoOrganize.Library.Services.LoggerServices;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.SettingsViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public sealed class LoggerSettingsViewModel : SettingsViewModelBase<LoggerConfig>
{
    public LoggerSettingsViewModel(IFileConfigManager configManager) : base(configManager)
    {
    }
}