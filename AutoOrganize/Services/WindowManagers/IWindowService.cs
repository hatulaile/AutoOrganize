using System;
using System.Threading.Tasks;
using AutoOrganize.ViewModels.Abstractions;
using Avalonia.Controls;

namespace AutoOrganize.Services.WindowManagers;

public interface IWindowService
{
    void Show<TWindowViewModel>(Window? ownerWindow = null, TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IWindowViewModel;

    void Show(Type viewModelType, Window? ownerWindow = null);

    void Show<TWindowViewModel>(object ownerViewModel, TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IWindowViewModel;

    void Show(Type viewModelType, object ownerViewModel);

    void Show<TWindowViewModel, TArgs>(TArgs args, Window? ownerWindow = null,
        TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IWindowViewModel<TArgs>;

    void Show<TWindowViewModel, TArgs>(TArgs arg, object ownerViewModel, TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IWindowViewModel<TArgs>;

    Task ShowDialog<TWindowViewModel>(Window ownerWindow, TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IWindowViewModel;

    Task ShowDialog<TWindowViewModel>(object ownerViewModel, TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IWindowViewModel;

    Task ShowDialog<TWindowViewModel, TArgs>(TArgs args, Window ownerWindow,
        TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IWindowViewModel<TArgs>;

    Task ShowDialog<TWindowViewModel, TArgs>(TArgs args, object ownerViewModel,
        TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IWindowViewModel<TArgs>;

    Task<TResult> ShowDialog<TWindowViewModel, TResult>(Window ownerWindow,
        TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IResultWindowViewModel<TResult>;

    Task<TResult> ShowDialog<TWindowViewModel, TResult>(object ownerViewModel,
        TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IResultWindowViewModel<TResult>;

    Task<TResult> ShowDialog<TWindowViewModel, TArgs, TResult>(TArgs args, Window ownerWindow,
        TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IWindowViewModel<TArgs, TResult>;

    Task<TResult> ShowDialog<TWindowViewModel, TArgs, TResult>(TArgs args, object ownerViewModel,
        TWindowViewModel? defaultViewModel = default)
        where TWindowViewModel : IWindowViewModel<TArgs, TResult>;

    void Close<TWindowViewModel>(TWindowViewModel viewModel)
        where TWindowViewModel : IWindowViewModel;

    void Close<TWindowViewModel, TResult>(TWindowViewModel viewModel, TResult result)
        where TWindowViewModel : IResultWindowViewModel<TResult>;
}