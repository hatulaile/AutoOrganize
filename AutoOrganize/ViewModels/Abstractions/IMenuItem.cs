using System.Collections.Generic;
using System.Windows.Input;
using AutoOrganize.Models.MenuItemViewModelContext;

namespace AutoOrganize.ViewModels.Abstractions;

public interface IMenuItem : IMenuEntry
{
    string Header { get; }

    bool IsEnable { get; }

    IReadOnlyList<IMenuEntry>? Children { get; }

    ICommand? ExecuteCommand { get; }
}

public interface IMenuItem<in TContext> : IMenuItem
    where TContext : IMenuItemContext
{
    void UpdateMenuItemStatus(TContext context);

    void IMenuEntry.UpdateMenuItemStatus(IMenuItemContext context) =>
        UpdateMenuItemStatus((TContext)context);
}