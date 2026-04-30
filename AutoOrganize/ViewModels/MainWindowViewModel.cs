using System.Linq;
using AutoOrganize.Models;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.Services.WindowManagers;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels;

[ViewModelRegistration(ViewModelLifetime.None)]
public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IWindowService _windowService;

    public RoutingState? RoutingState { get; }

    [ObservableProperty]
    public partial PageModel SelectedPage { get; set; }

    partial void OnSelectedPageChanged(PageModel value)
    {
        _navigationService.NavigateTo(HostScreens.Main, value.ViewModelType);
    }

    public AvaloniaList<PageModel> NavigationItems { get; } =
    [
        new("主页", string.Empty, typeof(HomeViewModel)),
        new("任务\n输出", string.Empty, typeof(ActivityViewModel)),
    ];

    [RelayCommand]
    private void ShowAbout()
    {
        _windowService.Show<AboutWindowViewModel>(this);
    }

    public MainWindowViewModel(INavigationService navigationService, [FromKeyedServices(HostScreens.Main)] RoutingState routingState, IWindowService windowService)
    {
        RoutingState = routingState;
        RoutingState.SetOwnerViewModel(this);
        _navigationService = navigationService;
        _windowService = windowService;
        SelectedPage = NavigationItems.First();
    }
}