using System;
using AutoOrganize.ViewModels.Abstractions;

namespace AutoOrganize.Services.NavigationServices;

public delegate void NavigatedHandler(object sender, NavigatedEventArgs ev);

public sealed class NavigatedEventArgs : EventArgs
{
    public IViewModel? OldViewModel { get; }

    public IViewModel NewViewModel { get; }

    public IParentViewModel? OwnerViewModel { get; }

    public NavigatedEventArgs(IViewModel? oldViewModel, IViewModel newViewModel, IParentViewModel? ownerViewModel)
    {
        OldViewModel = oldViewModel;
        NewViewModel = newViewModel;
        OwnerViewModel = ownerViewModel;
    }
}