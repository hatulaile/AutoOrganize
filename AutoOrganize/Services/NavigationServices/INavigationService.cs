using System;
using AutoOrganize.ViewModels.Abstractions;

namespace AutoOrganize.Services.NavigationServices;

public interface INavigationService
{
    void NavigateTo<TViewModel>(RoutingState routingState, TViewModel? defaultViewModel = default)
        where TViewModel : INavigationViewModel;

    void NavigateTo(RoutingState routingState, Type viewModelType);

    void NavigateTo<TViewModel, TArgs>(RoutingState routingState, TArgs args, TViewModel? defaultViewModel = default)
        where TViewModel : INavigationViewModel<TArgs>;

    void NavigateTo<TViewModel, TArgs>(RoutingState routingState, TArgs args, Type viewModelType);

    void Clear(RoutingState routingState);
}