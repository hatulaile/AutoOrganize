using System;
using AutoOrganize.ViewModels;
using Avalonia.Controls;

namespace AutoOrganize.Services.WindowManagers;

public partial class WindowService
{
    public Window? GetWindowByViewModel(object viewModel)
    {
        if (_windowByViewModel.TryGetValue(viewModel, out Window? window))
            return window;

        if (viewModel is ViewModelBase vm)
            return GetWindowByViewModel(vm);

        return null;
    }

    public Window GetRequiredWindowByViewModel(object viewModel)
    {
        Window? window = GetWindowByViewModel(viewModel);
        return window ?? throw new InvalidOperationException($"No window found for {viewModel.GetType().Name}");
    }

    public Window? GetWindowByViewModel(ViewModelBase viewModel)
    {
        if (_windowByViewModel.TryGetValue(viewModel, out var window))
            return window;

        ViewModelBase? currentViewModel = viewModel;
        do
        {
            if (_windowByViewModel.TryGetValue(currentViewModel, out window))
                return window;

            currentViewModel = currentViewModel.OwnerViewModel;
        } while (currentViewModel is not null);

        return null;
    }

    public Window GetRequiredWindowByViewModel(ViewModelBase viewModel)
    {
        Window? window = GetWindowByViewModel(viewModel);
        return window ?? throw new InvalidOperationException($"No window found for {viewModel.GetType().Name}");
    }
}