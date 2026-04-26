using System.Threading.Tasks;
using AutoOrganize.ViewModels;
using AutoOrganize.Views;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace AutoOrganize.Services.WindowManagers;

public partial class WindowService
{
    public Task ShowDialog<TWindowViewModel>(Window ownerWindow, TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel
    {
        TWindowViewModel viewModel = defaultViewModel ?? _serviceProvider.GetRequiredService<TWindowViewModel>();
        var dataContext = new HostWindowViewModel(viewModel);
        viewModel.OwnerViewModel = dataContext;
        var hostWindow = new HostWindow
        {
            DataContext = dataContext
        };

        hostWindow.Closed += (_, _) => viewModel.OnCloseWindow();
        viewModel.OnOpenWindow();
        return hostWindow.ShowDialog(ownerWindow);
    }

    public Task ShowDialog<TWindowViewModel>(object ownerViewModel, TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        return ShowDialog(ownerWindow, defaultViewModel);
    }

    public Task ShowDialog<TWindowViewModel, TArgs>(TArgs args, Window ownerWindow,
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

        hostWindow.Closed += (_, _) => viewModel.OnCloseWindow();
        viewModel.OnOpenWindow();
        viewModel.OnOpenWindow(args);
        return hostWindow.ShowDialog(ownerWindow);
    }

    public Task ShowDialog<TWindowViewModel, TArgs>(TArgs args, object ownerViewModel,
        TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel<TArgs>
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        return ShowDialog(args, ownerWindow, defaultViewModel);
    }

    public Task<TResult> ShowDialog<TWindowViewModel, TResult>(Window ownerWindow,
        TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IResultWindowViewModel<TResult>
    {
        TWindowViewModel viewModel = defaultViewModel ?? _serviceProvider.GetRequiredService<TWindowViewModel>();
        var dataContext = new HostWindowViewModel(viewModel);
        viewModel.OwnerViewModel = dataContext;
        var hostWindow = new HostWindow
        {
            DataContext = dataContext
        };

        hostWindow.Closed += (_, _) => viewModel.OnCloseWindow();
        viewModel.OnOpenWindow();
        return hostWindow.ShowDialog<TResult>(ownerWindow);
    }

    public Task<TResult> ShowDialog<TWindowViewModel, TResult>(object ownerViewModel,
        TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IResultWindowViewModel<TResult>
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        return ShowDialog<TWindowViewModel, TResult>(ownerWindow, defaultViewModel);
    }

    public Task<TResult> ShowDialog<TWindowViewModel, TArgs, TResult>(TArgs args, Window ownerWindow,
        TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel<TArgs, TResult>
    {
        TWindowViewModel viewModel = defaultViewModel ?? _serviceProvider.GetRequiredService<TWindowViewModel>();
        var dataContext = new HostWindowViewModel(viewModel);
        viewModel.OwnerViewModel = dataContext;
        var hostWindow = new HostWindow
        {
            DataContext = dataContext
        };

        hostWindow.Closed += (_, _) => viewModel.OnCloseWindow();
        viewModel.OnOpenWindow();
        viewModel.OnOpenWindow(args);
        return hostWindow.ShowDialog<TResult>(ownerWindow);
    }

    public Task<TResult> ShowDialog<TWindowViewModel, TArgs, TResult>(TArgs args, object ownerViewModel,
        TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel<TArgs, TResult>
    {
        Window ownerWindow = GetRequiredWindowByViewModel(ownerViewModel);
        return ShowDialog<TWindowViewModel, TArgs, TResult>(args, ownerWindow, defaultViewModel);
    }
}