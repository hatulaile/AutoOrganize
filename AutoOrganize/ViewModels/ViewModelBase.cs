using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoOrganize.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    private AvaloniaList<ViewModelBase> ChildViewModelsInternal => field ??= [];

    [ObservableProperty]
    public partial ViewModelBase? OwnerViewModel { get; internal set; }

    public IAvaloniaReadOnlyList<ViewModelBase> ChildViewModels => ChildViewModelsInternal;

    partial void OnOwnerViewModelChanging(ViewModelBase? oldValue, ViewModelBase? newValue)
    {
        oldValue?.RemoveChild(this);
        newValue?.AddChild(this);
    }

    private void AddChild(ViewModelBase childViewModel)
    {
        ChildViewModelsInternal.Add(childViewModel);
    }

    private void RemoveChild(ViewModelBase childViewModel)
    {
        ChildViewModelsInternal.Remove(childViewModel);
    }
}