using AutoOrganize.ViewModels;
using Avalonia.Controls;

namespace AutoOrganize.Services.WindowManagers;

public partial class WindowService
{
    public void Close<TWindowViewModel>(TWindowViewModel viewModel)
        where TWindowViewModel : ViewModelBase, IWindowViewModel
    {
        Window window = GetRequiredWindowByViewModel(viewModel);
        window.Close();
    }

    public void Close<TWindowViewModel, TResult>(TWindowViewModel viewModel, TResult result)
        where TWindowViewModel : ViewModelBase, IResultWindowViewModel<TResult>
    {
        Window window = GetRequiredWindowByViewModel(viewModel);
        window.Close(result);
    }
}