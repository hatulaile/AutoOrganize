using AutoOrganize.Services.NavigationServices;

namespace AutoOrganize.ViewModels.Abstractions;

public interface ISubNavigateViewModel : IParentViewModel
{
    RoutingState RoutingState { get; }
}