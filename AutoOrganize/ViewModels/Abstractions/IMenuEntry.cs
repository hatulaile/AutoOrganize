using AutoOrganize.Models.MenuItemViewModelContext;

namespace AutoOrganize.ViewModels.Abstractions;

public interface IMenuEntry
{
    bool IsVisible { get; }

    void UpdateMenuItemStatus(IMenuItemContext context);
}