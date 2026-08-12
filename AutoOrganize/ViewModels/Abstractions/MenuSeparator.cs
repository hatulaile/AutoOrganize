using System;
using AutoOrganize.Models.MenuItemViewModelContext;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoOrganize.ViewModels.Abstractions;

public interface IMenuSeparator : IMenuEntry;

public sealed partial class MenuSeparator : ObservableObject, IMenuSeparator
{
    private readonly Func<IMenuItemContext, bool>? _isVisible;

    [ObservableProperty]
    public partial bool IsVisible { get; set; }

    public MenuSeparator()
    {
    }

    public MenuSeparator(Func<IMenuItemContext, bool> isVisible)
    {
        _isVisible = isVisible;
    }

    public void UpdateMenuItemStatus(IMenuItemContext context)
    {
        if (_isVisible is null)
            return;

        IsVisible = _isVisible(context);
    }
}