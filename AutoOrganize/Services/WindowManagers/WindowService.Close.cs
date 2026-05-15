using System;
using AutoOrganize.ViewModels;
using Avalonia.Controls;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.Services.WindowManagers;

public partial class WindowService
{
    public void Close<TWindowViewModel>(TWindowViewModel viewModel)
        where TWindowViewModel : ViewModelBase, IWindowViewModel
    {
        Window window = GetRequiredWindowByViewModel(viewModel);
        if (CanCloseWindow(window)) window.Hide();
        else window.Close();
    }

    public void Close<TWindowViewModel, TResult>(TWindowViewModel viewModel, TResult result)
        where TWindowViewModel : ViewModelBase, IResultWindowViewModel<TResult>
    {
        Window window = GetRequiredWindowByViewModel(viewModel);
        //todo
        if (CanCloseWindow(window))
            throw new Exception();
        window.Close(result);
    }
}