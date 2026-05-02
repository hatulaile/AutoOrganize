using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AutoOrganize.Services.TopLevelServices;
using AutoOrganize.Services.WindowManagers;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public partial class AboutWindowViewModel : ViewModelBase, IWindowViewModel
{
    private readonly ILauncherServices _launcherServices;
    private readonly INotificationServices _notificationServices;
    private readonly ILogger<AboutWindowViewModel> _logger;

    public Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    public Version Version => Assembly.GetName().Version!;

    public string VersionString => $"{Version.ToString(3)} ({Version.Revision})";

    public string Configuration => Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()!.Configuration;

    public string InformationalVersion =>
        Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

    public string OSArchitecture => RuntimeInformation.OSArchitecture.ToString();

    public string RuntimeIdentifier => RuntimeInformation.RuntimeIdentifier;

    public string Framework => RuntimeInformation.FrameworkDescription;

    [RelayCommand]
    public async Task OpenUrl(string url)
    {
        _logger.LogDebug("尝试打开链接: {Url}", url);
        if (!await _launcherServices.LaunchUriAsync(new Uri(url), this))
        {
            _logger.LogWarning("打开链接失败: {Url}", url);
            _notificationServices.Show(new Notification("打开失败", $"打开 {url} 失败", NotificationType.Error), this);
        }
    }

    public AboutWindowViewModel(ILauncherServices launcherServices, INotificationServices notificationServices, ILogger<AboutWindowViewModel> logger)
    {
        _launcherServices = launcherServices;
        _notificationServices = notificationServices;
        _logger = logger;
    }
}