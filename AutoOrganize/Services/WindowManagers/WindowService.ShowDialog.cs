using System.Threading.Tasks;
using AutoOrganize.ViewModels.Abstractions;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace AutoOrganize.Services.WindowManagers;

public partial class WindowService
{
    public Task ShowDialog<TWindowViewModel>(Window ownerWindow, TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IWindowViewModel
    {
        TWindowViewModel viewModel = defaultViewModel ?? _serviceProvider.GetRequiredService<TWindowViewModel>();
        Window hostWindow = CreateOrGetWindow(viewModel);
        _windowByViewModel.TryAdd(hostWindow.DataContext!, hostWindow);
        viewModel.OnOpenWindow();
        return hostWindow.ShowDialog(ownerWindow);
    }

    public Task ShowDialog<TWindowViewModel>(object ownerViewModel, TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IWindowViewModel
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        return ShowDialog(ownerWindow, defaultViewModel);
    }

    public Task ShowDialog<TWindowViewModel, TArgs>(TArgs args, Window ownerWindow,
        TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IWindowViewModel<TArgs>
    {
        TWindowViewModel viewModel = defaultViewModel ?? _serviceProvider.GetRequiredService<TWindowViewModel>();
        Window hostWindow = CreateOrGetWindow(viewModel);
        _windowByViewModel.TryAdd(hostWindow.DataContext!, hostWindow);
        viewModel.OnOpenWindow();
        viewModel.OnOpenWindow(args);
        return hostWindow.ShowDialog(ownerWindow);
    }

    public Task ShowDialog<TWindowViewModel, TArgs>(TArgs args, object ownerViewModel,
        TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IWindowViewModel<TArgs>
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        return ShowDialog(args, ownerWindow, defaultViewModel);
    }

    public Task<TResult> ShowDialog<TWindowViewModel, TResult>(Window ownerWindow,
        TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IResultWindowViewModel<TResult>
    {
        TWindowViewModel viewModel = defaultViewModel ?? _serviceProvider.GetRequiredService<TWindowViewModel>();
        Window hostWindow = CreateOrGetWindow(viewModel);
        _windowByViewModel.TryAdd(hostWindow.DataContext!, hostWindow);
        viewModel.OnOpenWindow();
        return hostWindow.ShowDialog<TResult>(ownerWindow);
    }

    public Task<TResult> ShowDialog<TWindowViewModel, TResult>(object ownerViewModel,
        TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IResultWindowViewModel<TResult>
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        return ShowDialog<TWindowViewModel, TResult>(ownerWindow, defaultViewModel);
    }

    public Task<TResult> ShowDialog<TWindowViewModel, TArgs, TResult>(TArgs args, Window ownerWindow,
        TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IWindowViewModel<TArgs, TResult>
    {
        TWindowViewModel viewModel = defaultViewModel ?? _serviceProvider.GetRequiredService<TWindowViewModel>();
        Window hostWindow = CreateOrGetWindow(viewModel);
        _windowByViewModel.TryAdd(hostWindow.DataContext!, hostWindow);
        viewModel.OnOpenWindow();
        viewModel.OnOpenWindow(args);
        return hostWindow.ShowDialog<TResult>(ownerWindow);
    }

    public Task<TResult> ShowDialog<TWindowViewModel, TArgs, TResult>(TArgs args, object ownerViewModel,
        TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IWindowViewModel<TArgs, TResult>
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        return ShowDialog<TWindowViewModel, TArgs, TResult>(args, ownerWindow, defaultViewModel);
    }
}