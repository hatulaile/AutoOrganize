using AutoOrganize.Services.NavigationServices;
using Microsoft.Extensions.DependencyInjection;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public sealed partial class HomeViewModel : ViewModelBase, INavigationViewModel
{
    public RoutingState? RoutingState { get; }

    public HomeViewModel(INavigationService navigationService,
        [FromKeyedServices(HostScreens.Home)] RoutingState routingState)
    {
        RoutingState = routingState;
        RoutingState.SetOwnerViewModel(this);
        navigationService.NavigateTo<SelectFilesViewModel>(HostScreens.Home);
    }

    public void OnNavigatedTo()
    {
    }
}