using System;
using System.Linq;
using AutoOrganize.ViewLocators;
using AutoOrganize.ViewModels;
using AutoOrganize.Views;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace AutoOrganize.Services.WindowManagers;

public partial class WindowService
{
    public void Show<TWindowViewModel>(Window? ownerWindow = null, TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel
    {
        TWindowViewModel viewModel = defaultViewModel ?? _serviceProvider.GetRequiredService<TWindowViewModel>();
        Window hostWindow = CreateOrGetHostWindow(viewModel);
        _windowByViewModel.TryAdd(hostWindow.DataContext!, hostWindow);
        viewModel.OnOpenWindow();
        if (ownerWindow is null)
        {
            MainWindow.Closed += (_, _) => hostWindow.Close();
            hostWindow.Closed += (_, _) =>
            {
                viewModel.OnCloseWindow();
                if (hostWindow.DataContext is not null)
                    _windowByViewModel.TryRemove(hostWindow.DataContext, out _);
            };
        }
        else
        {
            hostWindow.Closed += (_, _) =>
            {
                viewModel.OnCloseWindow();
                if (hostWindow.DataContext is not null)
                    _windowByViewModel.TryRemove(hostWindow.DataContext, out _);
            };
        }

        ShowOrActiveWindow(hostWindow, ownerWindow);
    }

    public void Show<TWindowViewModel>(object ownerViewModel, TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        Show(ownerWindow, defaultViewModel);
    }

    public void Show<TWindowViewModel, TArgs>(TArgs args, Window? ownerWindow = null,
        TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel<TArgs>
    {
        TWindowViewModel viewModel = defaultViewModel ?? _serviceProvider.GetRequiredService<TWindowViewModel>();
        Window hostWindow = CreateOrGetHostWindow(viewModel);
        _windowByViewModel.TryAdd(hostWindow.DataContext!, hostWindow);
        viewModel.OnOpenWindow();
        viewModel.OnOpenWindow(args);
        if (ownerWindow is null)
        {
            MainWindow.Closed += (_, _) => hostWindow.Close();
            hostWindow.Closed += (_, _) =>
            {
                viewModel.OnCloseWindow();
                if (hostWindow.DataContext is not null)
                    _windowByViewModel.TryRemove(hostWindow.DataContext, out _);
            };
        }
        else
        {
            hostWindow.Closed += (_, _) =>
            {
                viewModel.OnCloseWindow();
                if (hostWindow.DataContext is not null)
                    _windowByViewModel.TryRemove(hostWindow.DataContext, out _);
            };
        }

        ShowOrActiveWindow(hostWindow, ownerWindow);
    }

    public void Show<TWindowViewModel, TArgs>(TArgs arg, object ownerViewModel,
        TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel<TArgs>
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        Show(ownerWindow, defaultViewModel);
    }

    private Window CreateOrGetHostWindow<TWindowViewModel>(TWindowViewModel viewModel)
        where TWindowViewModel : ViewModelBase, IWindowViewModel
    {
        if (!viewModel.AllowMultipleInstances)
        {
            Window? w = _windowByViewModel
                .FirstOrDefault(static kvp => kvp.Key.GetType() == typeof(TWindowViewModel)).Value;

            if (w is not null) return w;
        }

        if (ViewLocator.DefaultViewLocator.Build(viewModel) is not Window window)
        {
            //todo
            throw new Exception();
        }

        window.DataContext = viewModel;
        return window;
    }

    private void ShowOrActiveWindow(Window window, Window? ownerWindow = null)
    {
        if (window.IsVisible)
        {
            window.Activate();
            return;
        }

        if (ownerWindow is null) window.Show();
        else window.Show(ownerWindow);
    }
}