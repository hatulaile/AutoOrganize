using AutoOrganize.ViewModels.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoOrganize.Services.NavigationServices;

public partial class RoutingState : ObservableObject
{
    public IParentViewModel? OwnerViewModel { get; private set; }

    public IViewModel? CurrentPageViewModel
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public event NavigatedHandler? Navigated;

    public void SetOwnerViewModel(IParentViewModel owner)
    {
        OwnerViewModel = owner;
    }

    [RelayCommand]
    private void NavigateTo(IViewModel viewModel)
    {
        IViewModel? oldViewModel = CurrentPageViewModel;
        CurrentPageViewModel = viewModel;
        Navigated?.Invoke(this, new NavigatedEventArgs(oldViewModel, viewModel, OwnerViewModel));
    }

    [RelayCommand]
    private void Clear()
    {
        CurrentPageViewModel = null;
    }
}