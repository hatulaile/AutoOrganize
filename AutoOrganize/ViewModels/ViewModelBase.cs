using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoOrganize.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    public ViewModelBase? OwnerViewModel { get; internal set; }
}