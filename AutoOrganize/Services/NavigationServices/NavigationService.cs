using System;
using AutoOrganize.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AutoOrganize.Services.NavigationServices;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public void NavigateTo<TViewModel>(HostScreens screens, TViewModel? defaultViewModel = null)
        where TViewModel : ViewModelBase, INavigationViewModel
    {
        if (_serviceProvider.GetKeyedService<RoutingState>(screens) is not { } routingState)
            return;

        if ((defaultViewModel ?? _serviceProvider.GetService<TViewModel>()) is not { } viewModel)
            return;

        var oldViewModel = routingState.CurrentPageViewModel as INavigationViewModel;

        viewModel.OwnerViewModel = routingState.OwnerViewModel;
        if (ReferenceEquals(oldViewModel, viewModel))
            return;

        oldViewModel?.OnNavigatingFrom();
        viewModel.OnNavigatingTo();
        routingState.NavigateToCommand.Execute(viewModel);
        oldViewModel?.OnNavigatedFrom();
        viewModel.OnNavigatedTo();
    }

    public void NavigateTo(HostScreens screens, Type viewModelType)
    {
        if (_serviceProvider.GetKeyedService<RoutingState>(screens) is not { } routingState)
            return;

        object? vm = _serviceProvider.GetService(viewModelType);
        if (vm is not (INavigationViewModel navigationViewModel and ViewModelBase viewModelBase))
            return;

        var oldViewModel = routingState.CurrentPageViewModel as INavigationViewModel;

        viewModelBase.OwnerViewModel = routingState.OwnerViewModel;
        if (ReferenceEquals(oldViewModel, viewModelBase))
            return;

        oldViewModel?.OnNavigatingFrom();
        navigationViewModel.OnNavigatingTo();
        routingState.NavigateToCommand.Execute(viewModelBase);
        oldViewModel?.OnNavigatedFrom();
        navigationViewModel.OnNavigatedTo();
    }

    public void NavigateTo<TViewModel, TArgs>(HostScreens screens, TArgs args, TViewModel? defaultViewModel = null)
        where TViewModel : ViewModelBase, INavigationViewModel<TArgs>
    {
        if (_serviceProvider.GetKeyedService<RoutingState>(screens) is not { } routingState)
            return;

        if ((defaultViewModel ?? _serviceProvider.GetService<TViewModel>()) is not { } viewModel)
            return;

        var oldViewModel = routingState.CurrentPageViewModel as INavigationViewModel;

        viewModel.OwnerViewModel = routingState.OwnerViewModel;
        viewModel.NavigationParameter = args;
        if (ReferenceEquals(oldViewModel, viewModel))
        {
            viewModel.OnParameterChanged();
            return;
        }

        oldViewModel?.OnNavigatingFrom();
        viewModel.OnNavigatingTo();
        routingState.NavigateToCommand.Execute(viewModel);
        oldViewModel?.OnNavigatedFrom();
        viewModel.OnNavigatedTo();
    }

    public void NavigateTo<TViewModel, TArgs>(HostScreens screens, TArgs args, Type viewModelType)
    {
        if (_serviceProvider.GetKeyedService<RoutingState>(screens) is not { } routingState)
            return;

        object? vm = _serviceProvider.GetService(viewModelType);
        if (vm is not (INavigationViewModel<TArgs> navigationViewModel and ViewModelBase viewModelBase))
            return;

        var oldViewModel = routingState.CurrentPageViewModel as INavigationViewModel;

        viewModelBase.OwnerViewModel = routingState.OwnerViewModel;
        navigationViewModel.NavigationParameter = args;
        if (ReferenceEquals(oldViewModel, viewModelBase))
        {
            navigationViewModel.OnParameterChanged();
            return;
        }

        oldViewModel?.OnNavigatingFrom();
        navigationViewModel.OnNavigatingTo();
        routingState.NavigateToCommand.Execute(navigationViewModel);
        oldViewModel?.OnNavigatedFrom();
        navigationViewModel.OnNavigatedTo();
    }

    public void Clear(HostScreens screens)
    {
        if (_serviceProvider.GetKeyedService<RoutingState>(screens) is not { } routingState)
            return;

        var navigationViewModel = routingState.CurrentPageViewModel as INavigationViewModel;

        navigationViewModel?.OnNavigatingFrom();
        routingState.ClearCommand.Execute(null);
        navigationViewModel?.OnNavigatedFrom();
    }

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
}