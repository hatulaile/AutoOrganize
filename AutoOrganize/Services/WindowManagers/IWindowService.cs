using System.Threading.Tasks;
using AutoOrganize.ViewModels;
using Avalonia.Controls;

namespace AutoOrganize.Services.WindowManagers;

public interface IWindowService
{
    void Show<TWindowViewModel>(Window? ownerWindow = null, TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel;

    void Show<TWindowViewModel>(object ownerViewModel, TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel;

    void Show<TWindowViewModel, TArgs>(TArgs args, Window? ownerWindow = null,
        TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel<TArgs>;

    void Show<TWindowViewModel, TArgs>(TArgs arg, object ownerViewModel, TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel<TArgs>;

    Task ShowDialog<TWindowViewModel>(Window ownerWindow, TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel;

    Task ShowDialog<TWindowViewModel>(object ownerViewModel, TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel;

    Task ShowDialog<TWindowViewModel, TArgs>(TArgs args, Window ownerWindow, TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel<TArgs>;

    Task ShowDialog<TWindowViewModel, TArgs>(TArgs args, object ownerViewModel,
        TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel<TArgs>;

    Task<TResult> ShowDialog<TWindowViewModel, TResult>(Window ownerWindow, TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IResultWindowViewModel<TResult>;

    Task<TResult> ShowDialog<TWindowViewModel, TResult>(object ownerViewModel, TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IResultWindowViewModel<TResult>;

    Task<TResult> ShowDialog<TWindowViewModel, TArgs, TResult>(TArgs args, Window ownerWindow,
        TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel<TArgs, TResult>;

    Task<TResult> ShowDialog<TWindowViewModel, TArgs, TResult>(TArgs args, object ownerViewModel,
        TWindowViewModel? defaultViewModel = null)
        where TWindowViewModel : ViewModelBase, IWindowViewModel<TArgs, TResult>;

    void Close<TWindowViewModel>(TWindowViewModel viewModel)
        where TWindowViewModel : ViewModelBase, IWindowViewModel;

    void Close<TWindowViewModel, TResult>(TWindowViewModel viewModel, TResult result)
        where TWindowViewModel : ViewModelBase, IResultWindowViewModel<TResult>;
}