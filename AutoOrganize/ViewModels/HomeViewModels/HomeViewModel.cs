using AutoOrganize.Services.NavigationServices;
using AutoOrganize.ViewModels.Abstractions;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.HomeViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton,ViewModelLifetime.Singleton)]
public sealed partial class HomeViewModel : SubNavigateViewModelBase, INavigationViewModel
{
    public HomeViewModel(INavigationService navigationService)
    {
        navigationService.NavigateTo<SelectFilesViewModel>(RoutingState);
    }
}