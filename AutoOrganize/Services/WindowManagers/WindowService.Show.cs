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
        var dataContext = new HostWindowViewModel(viewModel);
        viewModel.OwnerViewModel = dataContext;
        var hostWindow = new HostWindow
        {
            DataContext = dataContext
        };

        _windowByViewModel.TryAdd(dataContext, hostWindow);
        viewModel.OnOpenWindow();
        if (ownerWindow is null)
        {
            MainWindow.Closed += (_, _) => hostWindow.Close();
            hostWindow.Closed += (_, _) =>
            {
                viewModel.OnCloseWindow();
                _windowByViewModel.TryRemove(viewModel, out _);
            };
            hostWindow.Show();
        }
        else
        {
            hostWindow.Closed += (_, _) =>
            {
                viewModel.OnCloseWindow();
                _windowByViewModel.TryRemove(viewModel, out _);
            };
            hostWindow.Show(ownerWindow);
        }
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
        var dataContext = new HostWindowViewModel(viewModel);
        viewModel.OwnerViewModel = dataContext;
        var hostWindow = new HostWindow
        {
            DataContext = dataContext
        };

        _windowByViewModel.TryAdd(dataContext, hostWindow);
        viewModel.OnOpenWindow();
        viewModel.OnOpenWindow(args);
        if (ownerWindow is null)
        {
            MainWindow.Closed += (_, _) => hostWindow.Close();
            hostWindow.Closed += (_, _) =>
            {
                viewModel.OnCloseWindow();
                _windowByViewModel.TryRemove(viewModel, out _);
            };
            hostWindow.Show();
        }
        else
        {
            hostWindow.Closed += (_, _) =>
            {
                viewModel.OnCloseWindow();
                _windowByViewModel.TryRemove(viewModel, out _);
            };
            hostWindow.Show(ownerWindow);
        }
    }

    public void Show<TWindowViewModel, TArgs>(TArgs arg, object ownerViewModel,
        TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel<TArgs>
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        Show(ownerWindow, defaultViewModel);
    }
}