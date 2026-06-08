using System;
using AutoOrganize.ViewModels.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AutoOrganize.Services.NavigationServices;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public void NavigateTo<TViewModel>(RoutingState routingState, TViewModel? defaultViewModel = default)
        where TViewModel : INavigationViewModel
    {
        if ((defaultViewModel ?? _serviceProvider.GetRequiredService<TViewModel>()) is not { } viewModel)
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

    public void NavigateTo(RoutingState routingState, Type viewModelType)
    {
        object vm = _serviceProvider.GetRequiredService(viewModelType);
        if (vm is not INavigationViewModel navigationViewModel)
            throw new InvalidOperationException($"{viewModelType.FullName} is not a navigationViewModel");

        var oldViewModel = routingState.CurrentPageViewModel as INavigationViewModel;

        navigationViewModel.OwnerViewModel = routingState.OwnerViewModel;
        if (ReferenceEquals(oldViewModel, navigationViewModel))
            return;

        oldViewModel?.OnNavigatingFrom();
        navigationViewModel.OnNavigatingTo();
        routingState.NavigateToCommand.Execute(navigationViewModel);
        oldViewModel?.OnNavigatedFrom();
        navigationViewModel.OnNavigatedTo();
    }

    public void NavigateTo<TViewModel, TArgs>(RoutingState routingState, TArgs args,
        TViewModel? defaultViewModel = default)
        where TViewModel : INavigationViewModel<TArgs>
    {
        if ((defaultViewModel ?? _serviceProvider.GetRequiredService<TViewModel>()) is not { } viewModel)
            return;

        var oldViewModel = routingState.CurrentPageViewModel as INavigationViewModel;

        viewModel.OwnerViewModel = routingState.OwnerViewModel;
        viewModel.OnParametersChanged(args);

        if (ReferenceEquals(oldViewModel, viewModel)) return;
        oldViewModel?.OnNavigatingFrom();
        viewModel.OnNavigatingTo();
        viewModel.OnNavigatingTo(args);
        routingState.NavigateToCommand.Execute(viewModel);
        oldViewModel?.OnNavigatedFrom();
        viewModel.OnNavigatedTo();
        viewModel.OnNavigatedTo(args);
    }

    public void NavigateTo<TViewModel, TArgs>(RoutingState routingState, TArgs args, Type viewModelType)
    {
        object vm = _serviceProvider.GetRequiredService(viewModelType);
        if (vm is not INavigationViewModel<TArgs> navigationViewModel)
            throw new InvalidOperationException($"{viewModelType.FullName} is not a navigationViewModel");

        var oldViewModel = routingState.CurrentPageViewModel as INavigationViewModel;

        navigationViewModel.OnParametersChanged(args);

        if (ReferenceEquals(oldViewModel, navigationViewModel)) return;
        navigationViewModel.OwnerViewModel = routingState.OwnerViewModel;
        oldViewModel?.OnNavigatingFrom();
        navigationViewModel.OnNavigatingTo();
        navigationViewModel.OnNavigatingTo(args);
        routingState.NavigateToCommand.Execute(navigationViewModel);
        oldViewModel?.OnNavigatedFrom();
        navigationViewModel.OnNavigatedTo();
        navigationViewModel.OnNavigatedTo(args);
    }

    public void Clear(RoutingState routingState)
    {
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