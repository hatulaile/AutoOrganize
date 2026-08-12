using System.Collections.Generic;
using System.Windows.Input;
using AutoOrganize.Models.MenuItemViewModelContext;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoOrganize.ViewModels.Abstractions;

public abstract partial class MenuItemBase : ViewModelBase, IMenuItem
{
    public abstract string Header { get; set; }

    public abstract IReadOnlyList<IMenuEntry>? Children { get; }

    public abstract ICommand? ExecuteCommand { get; }

    [ObservableProperty]
    public partial bool IsEnable { get; set; }

    [ObservableProperty]
    public partial bool IsVisible { get; set; }

    public abstract void UpdateMenuItemStatus(IMenuItemContext context);
}

public abstract partial class MenuItemBase<TContext> : ViewModelBase, IMenuItem<TContext>
    where TContext : IMenuItemContext
{
    public abstract string Header { get; set; }

    public abstract IReadOnlyList<IMenuEntry>? Children { get; }

    public abstract ICommand? ExecuteCommand { get; }

    [ObservableProperty]
    public partial bool IsEnable { get; set; }

    [ObservableProperty]
    public partial bool IsVisible { get; set; }

    public abstract void UpdateMenuItemStatus(TContext context);
}