using System;
using System.Collections.Generic;
using System.Windows.Input;
using AutoOrganize.Models.MenuItemViewModelContext;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoOrganize.ViewModels.Abstractions;

public partial class MenuItem : MenuItemBase
{
    private readonly Func<IMenuItemContext, bool>? _isVisible;

    [ObservableProperty]
    public override partial string Header { get; set; }

    public override IReadOnlyList<IMenuEntry>? Children { get; }

    public override ICommand? ExecuteCommand { get; }

    public MenuItem(string header, IRelayCommand<IMenuItemContext>? command,
        Func<IMenuItemContext, bool>? isVisible = null, IReadOnlyList<IMenuEntry>? children = null)
    {
        Header = header;
        ExecuteCommand = command;
        _isVisible = isVisible;
        Children = children;
    }

    public override void UpdateMenuItemStatus(IMenuItemContext context)
    {
        IsVisible = _isVisible?.Invoke(context) ?? true;
        IsEnable = ExecuteCommand?.CanExecute(context) ?? true;
    }
}

public partial class MenuItem<TContext> : MenuItemBase<TContext>
    where TContext : IMenuItemContext
{
    private readonly Func<TContext, bool>? _isVisible;

    [ObservableProperty]
    public override partial string Header { get; set; }

    public override IReadOnlyList<IMenuEntry>? Children { get; }

    public override ICommand? ExecuteCommand { get; }

    public MenuItem(string header, IRelayCommand<TContext>? command,
        Func<TContext, bool>? isVisible = null, IReadOnlyList<IMenuEntry>? children = null)
    {
        Header = header;
        ExecuteCommand = command;
        _isVisible = isVisible;
        Children = children;
    }

    public override void UpdateMenuItemStatus(TContext context)
    {
        IsVisible = _isVisible?.Invoke(context) ?? true;
        IsEnable = ExecuteCommand?.CanExecute(context) ?? true;
    }
}