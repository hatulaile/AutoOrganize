using System;
using System.Threading;
using System.Threading.Tasks;
using AutoOrganize.ViewModels.Abstractions;

namespace AutoOrganize.Services.NavigationServices;

public interface INavigationService
{
    void Replace<TViewModel>(RoutingState routingState)
        where TViewModel : INavigationViewModel;

    void Replace(RoutingState routingState, Type viewModelType);

    void Replace<TViewModel, TArgs>(RoutingState routingState, TArgs args)
        where TViewModel : INavigationViewModel<TArgs>;

    void Replace<TViewModel, TArgs>(RoutingState routingState, TArgs args, Type viewModelType);

    void Push<TViewModel>(RoutingState routingState)
        where TViewModel : INavigationViewModel;

    void Push(RoutingState routingState, Type viewModelType);

    void Push<TViewModel, TArgs>(RoutingState routingState, TArgs args)
        where TViewModel : INavigationViewModel<TArgs>;

    void Push<TViewModel, TArgs>(RoutingState routingState, TArgs args, Type viewModelType);

    Task<TResult> RequestAsync<TViewModel, TResult>(RoutingState routingState,
        CancellationToken token = default)
        where TViewModel : IResultNavigationViewModel<TResult>;

    Task<TResult> RequestAsync<TViewModel, TResult>(RoutingState routingState, Type viewModelType,
        CancellationToken token = default)
        where TViewModel : IResultNavigationViewModel<TResult>;

    Task<TResult> RequestAsync<TViewModel, TArgs, TResult>(RoutingState routingState, TArgs args,
        CancellationToken token = default)
        where TViewModel : IResultNavigationViewModel<TArgs, TResult>;

    Task<TResult> RequestAsync<TViewModel, TArgs, TResult>(RoutingState routingState, TArgs args, Type viewModelType,
        CancellationToken token = default)
        where TViewModel : IResultNavigationViewModel<TArgs, TResult>;

    void Pop<TResult>(RoutingState routingState, TResult result);

    void Pop(RoutingState routingState);

    void Clear(RoutingState routingState);
}
