using System.Linq;
using AutoOrganize.Models;
using AutoOrganize.Services.NavigationServices;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels;

[ViewModelRegistration(ViewModelLifetime.None)]
public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

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

    public MainWindowViewModel(INavigationService navigationService, [FromKeyedServices(HostScreens.Main)] RoutingState routingState)
    {
        RoutingState = routingState;
        RoutingState.SetOwnerViewModel(this);
        _navigationService = navigationService;
        SelectedPage = NavigationItems.First();
    }
}