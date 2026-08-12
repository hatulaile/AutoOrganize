using System;
using AutoOrganize.ViewModels.Abstractions;
using Avalonia.Controls;

namespace AutoOrganize.Services.WindowManagers;

public partial class WindowService
{
    public void Close(IWindowViewModel viewModel)
    {
        Window window = GetRequiredWindowByViewModel(viewModel);
        if (!CanCloseWindow(window)) window.Hide();
        else window.Close();
    }

    public void Close<TResult>(IResultWindowViewModel<TResult> viewModel, TResult result)
    {
        Window window = GetRequiredWindowByViewModel(viewModel);
        if (!CanCloseWindow(window))
            throw new InvalidOperationException();
        window.Close(result);
    }

    public void CloseWithParent(IViewModel viewModel)
    {
        if (viewModel.OwnerViewModel is null)
            throw new InvalidOperationException($"No owner window for {viewModel.GetType().Name}");

        Window window = GetRequiredWindowByViewModel(viewModel.OwnerViewModel);
        if (!CanCloseWindow(window)) window.Hide();
        else window.Close();
    }

    public void CloseWithParent<TResult>(IViewModel viewModel, TResult result)
    {
        if (viewModel.OwnerViewModel is null)
            throw new InvalidOperationException($"No owner window for {viewModel.GetType().Name}");


        Window window = GetRequiredWindowByViewModel(viewModel.OwnerViewModel);
        if (!CanCloseWindow(window))
            throw new InvalidOperationException();
        window.Close(result);
    }
}