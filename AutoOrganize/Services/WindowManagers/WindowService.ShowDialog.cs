using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AutoOrganize.ViewModels.Abstractions;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoOrganize.Services.WindowManagers;

public partial class WindowService
{
    public Task ShowDialog<TWindowViewModel>(Window ownerWindow,
        CancellationToken token = default)
        where TWindowViewModel : IWindowViewModel
    {
        TWindowViewModel viewModel = _serviceProvider.GetRequiredService<TWindowViewModel>();
        Window hostWindow = CreateOrGetWindow(viewModel);
        viewModel.OnOpenWindow();
        return ShowDialogAsync(hostWindow, ownerWindow, token);
    }

    public Task ShowDialog<TWindowViewModel>(object ownerViewModel,
        CancellationToken token = default)
        where TWindowViewModel : IWindowViewModel
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        return ShowDialog<TWindowViewModel>(ownerWindow, token);
    }

    public Task ShowDialog<TWindowViewModel, TArgs>(TArgs args, Window ownerWindow,
        CancellationToken token = default)
        where TWindowViewModel : IWindowViewModel<TArgs>
    {
        TWindowViewModel viewModel = _serviceProvider.GetRequiredService<TWindowViewModel>();
        Window hostWindow = CreateOrGetWindow(viewModel);
        viewModel.OnOpenWindow();
        viewModel.OnOpenWindow(args);
        return ShowDialogAsync(hostWindow, ownerWindow, token);
    }

    public Task ShowDialog<TWindowViewModel, TArgs>(TArgs args, object ownerViewModel,
        CancellationToken token = default)
        where TWindowViewModel : IWindowViewModel<TArgs>
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        return ShowDialog<TWindowViewModel, TArgs>(args, ownerWindow, token);
    }

    public Task<TResult> ShowDialog<TWindowViewModel, TResult>(Window ownerWindow,
        CancellationToken cancellationToken = default)
        where TWindowViewModel : IResultWindowViewModel<TResult>
    {
        TWindowViewModel viewModel = _serviceProvider.GetRequiredService<TWindowViewModel>();
        Window hostWindow = CreateOrGetWindow(viewModel);
        viewModel.CancellationToken = cancellationToken;
        viewModel.OnOpenWindow();
        return ShowDialogAsync<TResult>(hostWindow, ownerWindow, cancellationToken);
    }

    public Task<TResult> ShowDialog<TWindowViewModel, TResult>(object ownerViewModel,
        CancellationToken cancellationToken = default)
        where TWindowViewModel : IResultWindowViewModel<TResult>
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        return ShowDialog<TWindowViewModel, TResult>(ownerWindow, cancellationToken);
    }

    public Task<TResult> ShowDialog<TWindowViewModel, TArgs, TResult>(TArgs args, Window ownerWindow,
        CancellationToken cancellationToken = default)
        where TWindowViewModel : IResultWindowViewModel<TArgs, TResult>
    {
        TWindowViewModel viewModel = _serviceProvider.GetRequiredService<TWindowViewModel>();
        Window hostWindow = CreateOrGetWindow(viewModel);
        viewModel.CancellationToken = cancellationToken;
        viewModel.OnOpenWindow();
        viewModel.OnOpenWindow(args);
        return ShowDialogAsync<TResult>(hostWindow, ownerWindow, cancellationToken);
    }

    public Task<TResult> ShowDialog<TWindowViewModel, TArgs, TResult>(TArgs args, object ownerViewModel,
        CancellationToken cancellationToken = default)
        where TWindowViewModel : IResultWindowViewModel<TArgs, TResult>
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        return ShowDialog<TWindowViewModel, TArgs, TResult>(args, ownerWindow, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async Task ShowDialogAsync(Window hostWindow, Window ownerWindow, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            await hostWindow.ShowDialog(ownerWindow).WaitAsync(token);
        }
        catch (OperationCanceledException)
        {
            hostWindow.Close();
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "显示窗口 {WindowType} 时发生异常。", hostWindow.DataContext?.GetType().FullName);
            hostWindow.Close();
            throw;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async Task<TResult> ShowDialogAsync<TResult>
        (Window hostWindow, Window ownerWindow, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            return await hostWindow.ShowDialog<TResult>(ownerWindow).WaitAsync(token);
        }
        catch (OperationCanceledException)
        {
            hostWindow.Close();
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "显示结果窗口 {WindowType} 时发生异常。", hostWindow.DataContext?.GetType().FullName);
            hostWindow.Close();
            throw;
        }
    }
}
