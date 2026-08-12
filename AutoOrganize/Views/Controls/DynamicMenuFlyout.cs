using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using AutoOrganize.Models.MenuItemViewModelContext;
using AutoOrganize.ViewModels.Abstractions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using MenuItem = Avalonia.Controls.MenuItem;

namespace AutoOrganize.Views.Controls;

public class DynamicMenuFlyout : MenuFlyout
{
    static DynamicMenuFlyout()
    {
    }

    public static readonly StyledProperty<IMenuItemContext?> MenuItemContextProperty =
        AvaloniaProperty.Register<DynamicMenuFlyout, IMenuItemContext?>(nameof(MenuItemContext), inherits: true);

    public IMenuItemContext? MenuItemContext
    {
        get => GetValue(MenuItemContextProperty);
        set => SetValue(MenuItemContextProperty, value);
    }

    protected override Control CreatePresenter()
    {
        DynamicMenuFlyoutPresenter presenter = new DynamicMenuFlyoutPresenter
        {
            ItemsSource = this.Items,
            [!ItemsControl.ItemTemplateProperty] = this[!ItemTemplateProperty],
            [!ItemsControl.ItemContainerThemeProperty] = this[!ItemContainerThemeProperty],
            [!MenuItemContextProperty] = this[!MenuItemContextProperty]
        };
        return presenter;
    }

    protected override void OnOpening(CancelEventArgs args)
    {
        base.OnOpening(args);

        if (MenuItemContext is null || ItemsSource is null)
            return;

        bool isVisible = false;
        foreach (IMenuEntry menuEntry in ItemsSource.OfType<IMenuEntry>())
        {
            menuEntry.UpdateMenuItemStatus(MenuItemContext);
            if (menuEntry.IsVisible) isVisible = true;
        }

        args.Cancel = !isVisible;
    }
}

public class DynamicMenuFlyoutPresenter : MenuFlyoutPresenter
{
    private static readonly CompiledBinding IsVisibleBinding =
        CompiledBinding.Create<IMenuEntry, bool>(x => x.IsVisible);

    private static readonly CompiledBinding HeaderBinding =
        CompiledBinding.Create<IMenuItem, string?>(x => x.Header);

    private static readonly CompiledBinding ChildrenBinding =
        CompiledBinding.Create<IMenuItem, IEnumerable?>(x => x.Children);

    private static readonly CompiledBinding IsEnableBinding =
        CompiledBinding.Create<IMenuItem, bool>(x => x.IsEnable);

    private static readonly CompiledBinding CommandBinding =
        CompiledBinding.Create<IMenuItem, ICommand?>(x => x.ExecuteCommand);

    protected override Type StyleKeyOverride => typeof(MenuFlyoutPresenter);

    public static readonly StyledProperty<IMenuItemContext?> MenuItemContextProperty =
        DynamicMenuFlyout.MenuItemContextProperty.AddOwner<DynamicMenuFlyoutPresenter>();

    public IMenuItemContext? MenuItemContext
    {
        get => GetValue(MenuItemContextProperty);
        set => SetValue(MenuItemContextProperty, value);
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);

        if (container is not MenuItem menuItemControl || item is not IMenuEntry menuEntry)
            return;

        menuItemControl.Bind(IsVisibleProperty, IsVisibleBinding);
        switch (menuEntry)
        {
            case IMenuItem menuItem:
                menuItemControl.Bind(HeaderedSelectingItemsControl.HeaderProperty, HeaderBinding);
                menuItemControl.Bind(ItemsSourceProperty, ChildrenBinding);
                menuItemControl.Bind(IsEnabledProperty, IsEnableBinding);
                menuItemControl.Bind(MenuItem.CommandProperty, CommandBinding);
                menuItemControl[!MenuItem.CommandParameterProperty] = this[!MenuItemContextProperty];
                return;
            case IMenuSeparator:
                menuItemControl.Header = '-';
                menuItemControl.ClearValue(HeaderedSelectingItemsControl.HeaderProperty);
                menuItemControl.ClearValue(ItemsSourceProperty);
                menuItemControl.ClearValue(IsEnabledProperty);
                menuItemControl.ClearValue(MenuItem.CommandProperty);
                menuItemControl.ClearValue(MenuItem.CommandParameterProperty);
                return;
        }
    }
}