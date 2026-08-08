using System;
using AutoOrganize.ViewModels.Abstractions;

namespace AutoOrganize.Services.NavigationServices;

public delegate void NavigatedHandler(object sender, NavigatedEventArgs ev);

public sealed class NavigatedEventArgs : EventArgs
{
    public IViewModel? OldViewModel { get; }

    public IViewModel? NewViewModel { get; }

    public RoutingState RoutingState { get; }

    public NavigatedEventArgs(IViewModel? oldViewModel, IViewModel? newViewModel, RoutingState routingState)
    {
        OldViewModel = oldViewModel;
        NewViewModel = newViewModel;
        RoutingState = routingState;
    }
}