using System;
using AutoOrganize.ViewModels.Abstractions;
using Avalonia.Controls;

namespace AutoOrganize.Services.WindowManagers;

public partial class WindowService
{
    public void Close<TWindowViewModel>(TWindowViewModel viewModel)
        where TWindowViewModel : IWindowViewModel
    {
        Window window = GetRequiredWindowByViewModel(viewModel);
        if (CanCloseWindow(window)) window.Hide();
        else window.Close();
    }

    public void Close<TWindowViewModel, TResult>(TWindowViewModel viewModel, TResult result)
        where TWindowViewModel : IResultWindowViewModel<TResult>
    {
        Window window = GetRequiredWindowByViewModel(viewModel);
        //todo
        if (CanCloseWindow(window))
            throw new Exception();
        window.Close(result);
    }
}