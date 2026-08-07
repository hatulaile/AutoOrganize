using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AutoOrganize.ViewModels.Abstractions;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace AutoOrganize.Services.WindowManagers;

public partial class WindowService
{
    public Task ShowDialog<TWindowViewModel>(Window ownerWindow,
        TWindowViewModel? defaultViewModel = default, CancellationToken token = default)
        where TWindowViewModel : IWindowViewModel
    {
        TWindowViewModel viewModel = defaultViewModel ?? _serviceProvider.GetRequiredService<TWindowViewModel>();
        Window hostWindow = CreateOrGetWindow(viewModel);
        _windowByViewModel.TryAdd(hostWindow.DataContext!, hostWindow);
        viewModel.OnOpenWindow();
        return ShowDialogAsync(hostWindow, ownerWindow, token);
    }

    public Task ShowDialog<TWindowViewModel>(object ownerViewModel, TWindowViewModel? defaultViewModel = default,
        CancellationToken token = default)
        where TWindowViewModel : IWindowViewModel
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        return ShowDialog(ownerWindow, defaultViewModel, token);
    }

    public Task ShowDialog<TWindowViewModel, TArgs>(TArgs args, Window ownerWindow,
        TWindowViewModel? defaultViewModel = default, CancellationToken token = default)
        where TWindowViewModel : IWindowViewModel<TArgs>
    {
        TWindowViewModel viewModel = defaultViewModel ?? _serviceProvider.GetRequiredService<TWindowViewModel>();
        Window hostWindow = CreateOrGetWindow(viewModel);
        _windowByViewModel.TryAdd(hostWindow.DataContext!, hostWindow);
        viewModel.OnOpenWindow();
        viewModel.OnOpenWindow(args);
        return ShowDialogAsync(hostWindow, ownerWindow, token);
    }

    public Task ShowDialog<TWindowViewModel, TArgs>(TArgs args, object ownerViewModel,
        TWindowViewModel? defaultViewModel = default, CancellationToken token = default)
        where TWindowViewModel : IWindowViewModel<TArgs>
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        return ShowDialog(args, ownerWindow, defaultViewModel, token);
    }

    public Task<TResult> ShowDialog<TWindowViewModel, TResult>(Window ownerWindow,
        TWindowViewModel? defaultViewModel = default, CancellationToken cancellationToken = default)
        where TWindowViewModel : IResultWindowViewModel<TResult>
    {
        TWindowViewModel viewModel = defaultViewModel ?? _serviceProvider.GetRequiredService<TWindowViewModel>();
        Window hostWindow = CreateOrGetWindow(viewModel);
        _windowByViewModel.TryAdd(hostWindow.DataContext!, hostWindow);
        viewModel.CancellationToken = cancellationToken;
        viewModel.OnOpenWindow();
        return ShowDialogAsync<TResult>(hostWindow, ownerWindow, cancellationToken);
    }

    public Task<TResult> ShowDialog<TWindowViewModel, TResult>(object ownerViewModel,
        TWindowViewModel? defaultViewModel = default, CancellationToken cancellationToken = default)
        where TWindowViewModel : IResultWindowViewModel<TResult>
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        return ShowDialog<TWindowViewModel, TResult>(ownerWindow, defaultViewModel, cancellationToken);
    }

    public Task<TResult> ShowDialog<TWindowViewModel, TArgs, TResult>(TArgs args, Window ownerWindow,
        TWindowViewModel? defaultViewModel = default, CancellationToken cancellationToken = default)
        where TWindowViewModel : IResultWindowViewModel<TArgs, TResult>
    {
        TWindowViewModel viewModel = defaultViewModel ?? _serviceProvider.GetRequiredService<TWindowViewModel>();
        Window hostWindow = CreateOrGetWindow(viewModel);
        _windowByViewModel.TryAdd(hostWindow.DataContext!, hostWindow);
        viewModel.CancellationToken = cancellationToken;
        viewModel.OnOpenWindow();
        viewModel.OnOpenWindow(args);
        return ShowDialogAsync<TResult>(hostWindow, ownerWindow, cancellationToken);
    }

    public Task<TResult> ShowDialog<TWindowViewModel, TArgs, TResult>(TArgs args, object ownerViewModel,
        TWindowViewModel? defaultViewModel = default, CancellationToken cancellationToken = default)
        where TWindowViewModel : IResultWindowViewModel<TArgs, TResult>
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        return ShowDialog<TWindowViewModel, TArgs, TResult>(args, ownerWindow, defaultViewModel, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static  Task ShowDialogAsync
        (Window hostWindow, Window ownerWindow, CancellationToken token) =>
        hostWindow.ShowDialog(ownerWindow).WaitAsync(token);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static  Task<TResult> ShowDialogAsync<TResult>
        (Window hostWindow, Window ownerWindow, CancellationToken token) =>
        hostWindow.ShowDialog<TResult>(ownerWindow).WaitAsync(token);
}