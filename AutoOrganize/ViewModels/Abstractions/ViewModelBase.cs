using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoOrganize.ViewModels.Abstractions;

public abstract partial class ViewModelBase : ObservableObject, IViewModel
{
    [ObservableProperty]
    public partial IParentViewModel? OwnerViewModel { get; set; }

    partial void OnOwnerViewModelChanged(IParentViewModel? oldValue, IParentViewModel? newValue)
    {
        oldValue?.RemoveChild(this);
        newValue?.AddChild(this);
    }
}