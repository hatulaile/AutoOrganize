using System;
using System.Threading;
using System.Threading.Tasks;
using AutoOrganize.ViewModels.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AutoOrganize.Services.NavigationServices;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Replace<TViewModel>(RoutingState routingState)
        where TViewModel : INavigationViewModel
    {
        ReplaceInternal(routingState, _serviceProvider.GetRequiredService<TViewModel>());
    }

    public void Replace(RoutingState routingState, Type viewModelType)
    {
        object viewModel = _serviceProvider.GetRequiredService(viewModelType);
        if (viewModel is not INavigationViewModel navigationViewModel)
            throw new InvalidOperationException($"{viewModelType.FullName} is not a navigationViewModel");
        ReplaceInternal(routingState, navigationViewModel);
    }

    public void Replace<TViewModel, TArgs>(RoutingState routingState, TArgs args)
        where TViewModel : INavigationViewModel<TArgs>
    {
        ReplaceInternal(routingState, _serviceProvider.GetRequiredService<TViewModel>(), args);
    }

    public void Replace<TViewModel, TArgs>(RoutingState routingState, TArgs args, Type viewModelType)
    {
        object viewModel = _serviceProvider.GetRequiredService(viewModelType);
        if (viewModel is not INavigationViewModel<TArgs> navigationViewModel)
            throw new InvalidOperationException($"{viewModelType.FullName} is not a navigationViewModel");
        ReplaceInternal(routingState, navigationViewModel, args);
    }

    public void Push<TViewModel>(RoutingState routingState)
        where TViewModel : INavigationViewModel
    {
        PushInternal(routingState, _serviceProvider.GetRequiredService<TViewModel>(), null);
    }

    public void Push(RoutingState routingState, Type viewModelType)
    {
        object viewModel = _serviceProvider.GetRequiredService(viewModelType);
        if (viewModel is not INavigationViewModel navigationViewModel)
            throw new InvalidOperationException($"{viewModelType.FullName} is not a navigationViewModel");
        PushInternal(routingState, navigationViewModel, null);
    }

    public void Push<TViewModel, TArgs>(RoutingState routingState, TArgs args)
        where TViewModel : INavigationViewModel<TArgs>
    {
        PushInternal(routingState, _serviceProvider.GetRequiredService<TViewModel>(), args, null);
    }

    public void Push<TViewModel, TArgs>(RoutingState routingState, TArgs args, Type viewModelType)
    {
        object viewModel = _serviceProvider.GetRequiredService(viewModelType);
        if (viewModel is not INavigationViewModel<TArgs> navigationViewModel)
            throw new InvalidOperationException($"{viewModelType.FullName} is not a navigationViewModel");
        PushInternal(routingState, navigationViewModel, args, null);
    }

    public Task<TResult> RequestAsync<TViewModel, TResult>(RoutingState routingState,
        CancellationToken token = default)
        where TViewModel : IResultNavigationViewModel<TResult>
    {
        TViewModel vm = _serviceProvider.GetRequiredService<TViewModel>();
        vm.CancellationToken = token;
        TaskCompletion<TResult> completion = new(token);
        if (!PushInternal(routingState, vm, completion))
            completion.Cancel();
        return completion.Task;
    }

    public Task<TResult> RequestAsync<TViewModel, TResult>(RoutingState routingState, Type viewModelType,
        CancellationToken token = default)
        where TViewModel : IResultNavigationViewModel<TResult>
    {
        object viewModel = _serviceProvider.GetRequiredService(viewModelType);
        if (viewModel is not IResultNavigationViewModel<TResult> nav)
            throw new InvalidOperationException($"{viewModelType.FullName} is not a navigationViewModel");
        nav.CancellationToken = token;
        TaskCompletion<TResult> completion = new(token);
        if (!PushInternal(routingState, nav, completion))
            completion.Cancel();
        return completion.Task;
    }

    public Task<TResult> RequestAsync<TViewModel, TArgs, TResult>(RoutingState routingState, TArgs args,
        CancellationToken token = default)
        where TViewModel : IResultNavigationViewModel<TArgs, TResult>
    {
        TViewModel vm = _serviceProvider.GetRequiredService<TViewModel>();
        vm.CancellationToken = token;
        TaskCompletion<TResult> completion = new(token);
        if (!PushInternal(routingState, vm, args, completion))
            completion.Cancel();
        return completion.Task;
    }

    public Task<TResult> RequestAsync<TViewModel, TArgs, TResult>(RoutingState routingState, TArgs args,
        Type viewModelType, CancellationToken token = default)
        where TViewModel : IResultNavigationViewModel<TArgs, TResult>
    {
        object viewModel = _serviceProvider.GetRequiredService(viewModelType);
        if (viewModel is not IResultNavigationViewModel<TArgs, TResult> nav)
            throw new InvalidOperationException($"{viewModelType.FullName} is not a navigationViewModel");
        nav.CancellationToken = token;
        TaskCompletion<TResult> completion = new(token);
        if (!PushInternal(routingState, nav, args, completion))
            completion.Cancel();
        return completion.Task;
    }

    public void Pop<TResult>(RoutingState routingState, TResult result) =>
        PopInternal(routingState, result);

    public void Pop(RoutingState routingState) =>
        PopInternal(routingState);

    public void Clear(RoutingState routingState)
    {
        var nav = routingState.CurrentPageViewModel as INavigationViewModel;
        nav?.OnNavigatingFrom();
        routingState.Clear();
        nav?.OnNavigatedFrom();
    }

    private static bool ReplaceInternal<TViewModel>(RoutingState routingState, TViewModel vm)
        where TViewModel : INavigationViewModel
    {
        var oldViewModel = routingState.CurrentPageViewModel as INavigationViewModel;

        vm.OwnerViewModel = routingState.OwnerViewModel;
        if (ReferenceEquals(oldViewModel, vm))
            return false;

        oldViewModel?.OnNavigatingFrom();
        vm.OnNavigatingTo();
        routingState.Replace(new PageRecord(vm, null));
        oldViewModel?.OnNavigatedFrom();
        vm.OnNavigatedTo();
        return true;
    }

    private static bool ReplaceInternal<TViewModel, TArgs>(RoutingState routingState, TViewModel vm, TArgs args)
        where TViewModel : INavigationViewModel<TArgs>
    {
        var oldViewModel = routingState.CurrentPageViewModel as INavigationViewModel;

        vm.OwnerViewModel = routingState.OwnerViewModel;
        vm.OnParametersChanged(args);
        if (ReferenceEquals(vm, oldViewModel))
            return false;

        oldViewModel?.OnNavigatingFrom();
        vm.OnNavigatingTo();
        vm.OnNavigatingTo(args);
        routingState.Replace(new PageRecord(vm, null));
        oldViewModel?.OnNavigatedFrom();
        vm.OnNavigatedTo();
        vm.OnNavigatedTo(args);
        return true;
    }

    private static bool PushInternal<TViewModel>(RoutingState routingState, TViewModel vm,
        INavigationCompletion? result)
        where TViewModel : INavigationViewModel
    {
        var oldViewModel = routingState.CurrentPageViewModel as INavigationViewModel;

        vm.OwnerViewModel = routingState.OwnerViewModel;
        if (ReferenceEquals(oldViewModel, vm))
            return false;

        vm.OnNavigatingTo();
        routingState.Push(new PageRecord(vm, result));
        vm.OnNavigatedTo();
        return true;
    }

    private static bool PushInternal<TViewModel, TArgs>(RoutingState routingState, TViewModel vm, TArgs args,
        INavigationCompletion? result)
        where TViewModel : INavigationViewModel<TArgs>
    {
        var oldViewModel = routingState.CurrentPageViewModel as INavigationViewModel;

        vm.OwnerViewModel = routingState.OwnerViewModel;
        vm.OnParametersChanged(args);

        if (ReferenceEquals(vm, oldViewModel))
            return false;
        vm.OnNavigatingTo();
        vm.OnNavigatingTo(args);
        routingState.Push(new PageRecord(vm, result));
        vm.OnNavigatedTo();
        vm.OnNavigatedTo(args);
        return true;
    }

    private static void PopInternal(RoutingState routingState)
    {
        IViewModel? oldViewModel = routingState.CurrentPageViewModel;
        if (oldViewModel is null) return;
        var nav = oldViewModel as INavigationViewModel;
        nav?.OnNavigatingFrom();
        routingState.Pop();
        nav?.OnNavigatedFrom();
    }

    private static void PopInternal<TResult>(RoutingState routingState, TResult result)
    {
        IViewModel? oldViewModel = routingState.CurrentPageViewModel;
        if (oldViewModel is null) return;
        var nav = oldViewModel as INavigationViewModel;
        nav?.OnNavigatingFrom();
        routingState.Pop(result);
        nav?.OnNavigatedFrom();
    }
}