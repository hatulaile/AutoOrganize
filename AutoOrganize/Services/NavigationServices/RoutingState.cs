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

    public void SetOwnerViewModel(ViewModelBase owner)
    {
        OwnerViewModel = owner;
    }

    [RelayCommand]
    private void NavigateTo(ViewModelBase viewModel)
    {
        CurrentPageViewModel = viewModel;
    }

    [RelayCommand]
    private void Clear()
    {
        CurrentPageViewModel = null;
    }
}