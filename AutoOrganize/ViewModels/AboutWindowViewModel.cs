using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AutoOrganize.Services.TopLevelServices;
using AutoOrganize.Services.WindowManagers;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Input;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public partial class AboutWindowViewModel : ViewModelBase, IWindowViewModel
{
    private readonly ILauncherServices _launcherServices;
    private readonly INotificationServices _notificationServices;

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
        if (!await _launcherServices.LaunchUriAsync(new Uri(url), this))
        {
            _notificationServices.Show(new Notification("打开失败", $"打开 {url} 失败", NotificationType.Error), this);
        }
    }

    public AboutWindowViewModel(ILauncherServices launcherServices, INotificationServices notificationServices)
    {
        _launcherServices = launcherServices;
        _notificationServices = notificationServices;
    }
}