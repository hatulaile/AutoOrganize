using AutoOrganize.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoOrganize.Services.NavigationServices;

public partial class RoutingState : ObservableObject
{
    public ViewModelBase? OwnerViewModel { get; private set; }

    public ViewModelBase? CurrentPageViewModel
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public event NavigatedHandler? Navigated;

    public void SetOwnerViewModel(ViewModelBase owner)
    {
        OwnerViewModel = owner;
    }

    [RelayCommand]
    private void NavigateTo(ViewModelBase viewModel)
    {
        ViewModelBase? oldViewModel = CurrentPageViewModel;
        CurrentPageViewModel = viewModel;
        Navigated?.Invoke(this, new NavigatedEventArgs(oldViewModel, viewModel, OwnerViewModel));
    }

    [RelayCommand]
    private void Clear()
    {
        CurrentPageViewModel = null;
    }
}