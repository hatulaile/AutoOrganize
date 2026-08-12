using System;
using System.Threading;
using System.Threading.Tasks;
using AutoOrganize.ViewModels.Abstractions;
using Avalonia.Controls;

namespace AutoOrganize.Services.WindowManagers;

public interface IWindowService
{
    void Show<TWindowViewModel>(Window? ownerWindow = null)
        where TWindowViewModel : IWindowViewModel;

    void Show(Type viewModelType, Window? ownerWindow = null);

    void Show<TWindowViewModel>(object ownerViewModel)
        where TWindowViewModel : IWindowViewModel;

    void Show(Type viewModelType, object ownerViewModel);

    void Show<TWindowViewModel, TArgs>(TArgs args, Window? ownerWindow = null)
        where TWindowViewModel : IWindowViewModel<TArgs>;

    void Show<TWindowViewModel, TArgs>(TArgs arg, object ownerViewModel)
        where TWindowViewModel : IWindowViewModel<TArgs>;

    Task ShowDialog<TWindowViewModel>(Window ownerWindow,
        CancellationToken token = default)
        where TWindowViewModel : IWindowViewModel;

    Task ShowDialog<TWindowViewModel>(object ownerViewModel,
        CancellationToken token = default)
        where TWindowViewModel : IWindowViewModel;

    Task ShowDialog<TWindowViewModel, TArgs>(TArgs args, Window ownerWindow,
        CancellationToken token = default)
        where TWindowViewModel : IWindowViewModel<TArgs>;

    Task ShowDialog<TWindowViewModel, TArgs>(TArgs args, object ownerViewModel,
        CancellationToken token = default)
        where TWindowViewModel : IWindowViewModel<TArgs>;

    Task<TResult> ShowDialog<TWindowViewModel, TResult>(Window ownerWindow,
        CancellationToken token = default)
        where TWindowViewModel : IResultWindowViewModel<TResult>;

    Task<TResult> ShowDialog<TWindowViewModel, TResult>(object ownerViewModel,
        CancellationToken token = default)
        where TWindowViewModel : IResultWindowViewModel<TResult>;

    Task<TResult> ShowDialog<TWindowViewModel, TArgs, TResult>(TArgs args, Window ownerWindow,
        CancellationToken token = default)
        where TWindowViewModel : IResultWindowViewModel<TArgs, TResult>;

    Task<TResult> ShowDialog<TWindowViewModel, TArgs, TResult>(TArgs args, object ownerViewModel,
        CancellationToken token = default)
        where TWindowViewModel : IResultWindowViewModel<TArgs, TResult>;

    void Close(IWindowViewModel viewModel);

    void Close<TResult>(IResultWindowViewModel<TResult> viewModel, TResult result);

    void CloseWithParent(IViewModel viewModel);

    void CloseWithParent<TResult>(IViewModel viewModel, TResult result);
}