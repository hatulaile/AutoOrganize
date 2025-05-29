using System;
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
    public partial IPageModel SelectedPage { get; set; }

    public AvaloniaList<IPageModel> NavigationItems { get; } =
    [
        new PageModel<HomeViewModel>("主页", string.Empty),
        new PageModel<LogViewModel>("日志", string.Empty),
    ];

    public MainWindowViewModel(INavigationService navigationService,
        [FromKeyedServices(HostScreens.Main)] RoutingState routingState, IWindowService windowService,
        ILogger<MainWindowViewModel> logger)
    {
        RoutingState = routingState;
        RoutingState.SetOwnerViewModel(this);
        _navigationService = navigationService;
        _windowService = windowService;
        _logger = logger;
        SelectedPage = NavigationItems.First();
        NavigateToPageCommand.Execute(SelectedPage);
        _logger.LogDebug("MainWindowViewModel 初始化完成，默认页面: {PageName}", SelectedPage.Title);
    }

    [RelayCommand]
    private void NavigateToPage(IPageModel pageModel)
    {
        if (pageModel.ViewModelType is null) return;
        _logger.LogDebug("导航到页面: {PageName} ({ViewModelType})", pageModel.Title, pageModel.ViewModelType.Name);
        _navigationService.NavigateTo(HostScreens.Main, pageModel.ViewModelType);
    }

    [RelayCommand]
    private void ShowWindow(Type type)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("用户打开了 {TypeName} 窗口", type.FullName);
        _windowService.Show(type, this);
    }
}