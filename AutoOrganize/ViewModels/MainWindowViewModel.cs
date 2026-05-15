using System.Linq;
using AutoOrganize.Models;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.Services.WindowManagers;
using AutoOrganize.ViewModels.AboutViewModels;
using AutoOrganize.ViewModels.HomeViewModels;
using AutoOrganize.ViewModels.LogViewModels;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels;

[ViewModelRegistration(ViewModelLifetime.None)]
public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IWindowService _windowService;
    private readonly ILogger<MainWindowViewModel> _logger;

    public RoutingState? RoutingState { get; }

    [ObservableProperty]
    public partial PageModel SelectedPage { get; set; }

    partial void OnSelectedPageChanged(PageModel value)
    {
        _logger.LogDebug("导航到页面: {PageName} ({ViewModelType})", value.Title, value.ViewModelType.Name);
        _navigationService.NavigateTo(HostScreens.Main, value.ViewModelType);
    }

    public AvaloniaList<PageModel> NavigationItems { get; } =
    [
        new("主页", string.Empty, typeof(HomeViewModel)),
        new("日志", string.Empty, typeof(LogViewModel)),
    ];

    [RelayCommand]
    private void ShowAbout()
    {
        _logger.LogDebug("用户打开了关于窗口");
        _windowService.Show<AboutWindowViewModel>(this);
    }

    public MainWindowViewModel(INavigationService navigationService, [FromKeyedServices(HostScreens.Main)] RoutingState routingState, IWindowService windowService, ILogger<MainWindowViewModel> logger)
    {
        RoutingState = routingState;
        RoutingState.SetOwnerViewModel(this);
        _navigationService = navigationService;
        _windowService = windowService;
        _logger = logger;
        SelectedPage = NavigationItems.First();
        _logger.LogDebug("MainWindowViewModel 初始化完成，默认页面: {PageName}", SelectedPage.Title);
    }
}