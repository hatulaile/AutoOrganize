using AutoOrganize.Services.NavigationServices;

namespace AutoOrganize.ViewModels.Abstractions;

public abstract partial class SubNavigateViewModelBase : ParentViewModelBase, ISubNavigateViewModel
{
    public RoutingState RoutingState { get; }

    protected SubNavigateViewModelBase()
    {
        RoutingState = new RoutingState();
        RoutingState.SetOwnerViewModel(this);
    }
}