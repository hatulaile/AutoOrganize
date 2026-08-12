using System;
using System.Diagnostics.CodeAnalysis;
using AutoOrganize.ViewModels.Abstractions;
using Avalonia.Controls;

namespace AutoOrganize.Services.WindowManagers;

public partial class WindowService
{
    public Window? GetWindowByViewModel(object viewModel) =>
        viewModel is IViewModel vm ? GetWindowByViewModel(vm) : null;

    public Window GetRequiredWindowByViewModel(object viewModel)
    {
        Window? window = GetWindowByViewModel(viewModel);
        return window ?? throw new InvalidOperationException($"No window found for {viewModel.GetType().Name}");
    }

    public Window? GetWindowByViewModel(IViewModel viewModel)
    {
        IViewModel? currentViewModel = viewModel;
        do
        {
            if (currentViewModel is IWindowViewModel windowViewModel &&
                TryGetWindowForViewModel(windowViewModel, out Window? window))
            {
                return window;
            }

            currentViewModel = currentViewModel.OwnerViewModel;
        } while (currentViewModel is not null);

        return null;
    }

    public Window GetRequiredWindowByViewModel(IViewModel viewModel)
    {
        Window? window = GetWindowByViewModel(viewModel);
        return window ?? throw new InvalidOperationException($"No window found for {viewModel.GetType().Name}");
    }

    public Window? GetWindowByViewModel(IWindowViewModel viewModel) =>
        TryGetWindowForViewModel(viewModel, out Window? window) ? window : null;

    public Window GetRequiredWindowByViewModel(IWindowViewModel viewModel)
    {
        Window? window = GetWindowByViewModel(viewModel);
        return window ?? throw new InvalidOperationException($"No window found for {viewModel.GetType().Name}");
    }

    private bool TryGetWindowForViewModel(IWindowViewModel viewModel, [NotNullWhen(true)] out Window? window)
    {
        if (ReferenceEquals(viewModel, MainWindow.DataContext))
        {
            window = MainWindow;
            return true;
        }

        if (_windowByViewModel.TryGetValue(viewModel, out window))
            return true;

        window = null;
        return false;
    }
}