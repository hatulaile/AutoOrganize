using System;
using AutoOrganize.ViewModels;

namespace AutoOrganize.Services.NavigationServices;

public delegate void NavigatedHandler(object sender, NavigatedEventArgs ev);

public sealed class NavigatedEventArgs : EventArgs
{
    public ViewModelBase? OldViewModel { get; }

    public ViewModelBase NewViewModel { get; }

    public ViewModelBase? OwnerViewModel { get; }

    public NavigatedEventArgs(ViewModelBase? oldViewModel, ViewModelBase newViewModel, ViewModelBase? ownerViewModel)
    {
        OldViewModel = oldViewModel;
        NewViewModel = newViewModel;
        OwnerViewModel = ownerViewModel;
    }
}