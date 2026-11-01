using System;
using System.Linq;
using AutoOrganize.ViewLocators;
using AutoOrganize.ViewModels.Abstractions;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace AutoOrganize.Services.WindowManagers;

public partial class WindowService
{
    public void Show<TWindowViewModel>(Window? ownerWindow = null)
        where TWindowViewModel : IWindowViewModel
    {
        TWindowViewModel viewModel = _serviceProvider.GetRequiredService<TWindowViewModel>();
        Window hostWindow = CreateOrGetWindow(viewModel);
        viewModel.OnOpenWindow();
        if (ownerWindow is null)
        {
            EventHandler? handler = null;
            handler = (_, _) =>
            {
                hostWindow.Close();
                MainWindow.Closed -= handler;
            };
            MainWindow.Closed += handler;
        }

        ShowOrActiveWindow(hostWindow, ownerWindow);
    }

    public void Show(Type viewModelType, Window? ownerWindow = null)
    {
        if (_serviceProvider.GetRequiredService(viewModelType) is not IWindowViewModel viewModel)
        {
            throw new InvalidOperationException(
                $"类型 '{viewModelType.FullName}' 未实现 {nameof(IWindowViewModel)}。");
        }

        Window hostWindow = CreateOrGetWindow(viewModel);
        viewModel.OnOpenWindow();
        if (ownerWindow is null)
        {
            EventHandler? handler = null;
            handler = (_, _) =>
            {
                hostWindow.Close();
                MainWindow.Closed -= handler;
            };
            MainWindow.Closed += handler;
        }

        ShowOrActiveWindow(hostWindow, ownerWindow);
    }

    public void Show<TWindowViewModel>(object ownerViewModel)
        where TWindowViewModel : IWindowViewModel
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        Show<TWindowViewModel>(ownerWindow);
    }

    public void Show(Type viewModelType, object ownerViewModel)
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        Show(viewModelType, ownerWindow);
    }

    public void Show<TWindowViewModel, TArgs>(TArgs args, Window? ownerWindow = null)
        where TWindowViewModel : IWindowViewModel<TArgs>
    {
        TWindowViewModel viewModel = _serviceProvider.GetRequiredService<TWindowViewModel>();
        Window hostWindow = CreateOrGetWindow(viewModel);
        viewModel.OnOpenWindow();
        viewModel.OnOpenWindow(args);
        if (ownerWindow is null)
        {
            EventHandler? handler = null;
            handler = (_, _) =>
            {
                hostWindow.Close();
                MainWindow.Closed -= handler;
            };
            MainWindow.Closed += handler;
        }

        ShowOrActiveWindow(hostWindow, ownerWindow);
    }

    public void Show<TWindowViewModel, TArgs>(TArgs arg, object ownerViewModel)
        where TWindowViewModel : IWindowViewModel<TArgs>
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        Show<TWindowViewModel, TArgs>(arg, ownerWindow);
    }

    private Window CreateOrGetWindow<TWindowViewModel>(TWindowViewModel viewModel)
        where TWindowViewModel : IWindowViewModel
    {
        if (!viewModel.AllowMultipleInstances)
        {
            Window? w = _windowByViewModel
                .FirstOrDefault(kvp => kvp.Key.GetType() == viewModel.GetType()).Value;

            if (w is not null) return w;
        }

        if (ViewLocator.DefaultViewLocator.Build(viewModel) is not Window window)
        {
            throw new InvalidOperationException(
                $"The view for ViewModel '{viewModel.GetType().Name}' could not be resolved ");
        }

        if (!ReferenceEquals(window.DataContext, viewModel))
            window.DataContext = viewModel;

        InitializeIfNeeded(window);
        return window;
    }

    private void ShowOrActiveWindow(Window window, Window? ownerWindow = null)
    {
        if (window.DataContext is IViewModel vm)
        {
            IParentViewModel? ownerViewModel;
            if (ownerWindow is not null) ownerViewModel = ownerWindow.DataContext as IParentViewModel;
            else ownerViewModel = window.Owner?.DataContext as IParentViewModel;

            if (ownerViewModel is not null && !ReferenceEquals(ownerViewModel, vm))
                vm.OwnerViewModel = ownerViewModel;
        }

        if (window.IsVisible)
        {
            if (window.WindowState is WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
                return;
            }

            window.Activate();
            return;
        }

        if (ownerWindow is null) window.Show();
        else window.Show(ownerWindow);
    }

    private void InitializeIfNeeded(Window window)
    {
        if (window.IsLoaded)
            return;

        if (window.DataContext is not null)
            _windowByViewModel.TryAdd(window.DataContext!, window);
        window.Closing += (windowObj, ev) =>
        {
            var win = (Window?)windowObj ?? throw new ArgumentException("Window cannot be null", nameof(windowObj));
            var viewModel = win.DataContext as IWindowViewModel;

            (win.DataContext as IViewModel)?.OwnerViewModel = null;
            viewModel?.OnCloseWindow();
            // 这里有几个逻辑
            // 1. 如果是主窗体则不直接关闭，
            // 2. 如果主窗体关闭了也直接关闭
            // 3. 如果是瞬态的也直接关闭，
            // 4. 如果是系统通知的也直接关闭，
            // 5. 如果 _windowByViewModel 不包含这个 vm 也直接关闭
            if (!_isMainClosing && !ReferenceEquals(win, MainWindow) && !CanCloseWindow(win) &&
                ev.CloseReason is not (WindowCloseReason.ApplicationShutdown or WindowCloseReason.OSShutdown) &&
                viewModel is not null && _windowByViewModel.ContainsKey(viewModel))
            {
                ev.Cancel = true;
                win.Hide();
                return;
            }

            if (win.DataContext is not null)
                _windowByViewModel.TryRemove(win.DataContext, out _);
        };
    }
}