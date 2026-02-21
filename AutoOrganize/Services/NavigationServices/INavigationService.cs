using System;
using AutoOrganize.ViewModels;

namespace AutoOrganize.Services.NavigationServices;

public interface INavigationService
{
    void NavigateTo<TViewModel>(HostScreens screens, TViewModel? defaultViewModel = null)
        where TViewModel : ViewModelBase, INavigationViewModel;

    void NavigateTo(HostScreens screens, Type viewModelType);

    void NavigateTo<TViewModel, TArgs>(HostScreens screens, TArgs args, TViewModel? defaultViewModel = null)
        where TViewModel : ViewModelBase, INavigationViewModel<TArgs>;

    void NavigateTo<TViewModel, TArgs>(HostScreens screens, TArgs args, Type viewModelType);

    void Clear(HostScreens screens);
}