// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

//此处使用了 https://github.com/reactiveui/ReactiveUI.Avalonia/blob/main/src/ReactiveUI.Avalonia/RoutedViewHost.cs 的部分源码
//在 MIT 许可证的允许下进行修改和使用
//完整的许可证 https://github.com/reactiveui/ReactiveUI.Avalonia?tab=MIT-1-ov-file
//感谢 ReactiveUI.Avalonia 项目提供的源码参考

using System;
using System.ComponentModel;
using AutoOrganize.Services.NavigationServices;
using AutoOrganize.ViewLocators;
using AutoOrganize.ViewModels;
using Avalonia;
using Avalonia.Controls;

namespace AutoOrganize.Views.Controls;

public class RoutedViewHost : TransitioningContentControl
{
    public static readonly StyledProperty<RoutingState?> RouterProperty =
        AvaloniaProperty.Register<RoutedViewHost, RoutingState?>(nameof(Router));

    public static readonly StyledProperty<Control> DefaultContentProperty =
        AvaloniaProperty.Register<RoutedViewHost, Control>(nameof(DefaultContent), defaultValue: new TextBlock
        {
            Text =
                "Not Found: No default content set for RoutedViewHost. Please set the DefaultContent property to specify what should be shown when no view is found."
        });

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs ev)
    {
        base.OnPropertyChanged(ev);
        if (ev.Property == RouterProperty)
        {
            if (ev.OldValue is RoutingState oldRoutingState)
                oldRoutingState.PropertyChanged -= OnRouterPropertyChanged;

            if (ev.NewValue is RoutingState newRoutingState)
            {
                newRoutingState.PropertyChanged += OnRouterPropertyChanged;
                NavigateToViewModel(newRoutingState.CurrentPageViewModel);
            }

            return;
        }

        if (ev.Property == ContentProperty && ev.NewValue is RoutingState routingState)
        {
            Router = routingState;
        }
    }

    public void OnRouterPropertyChanged(object? sender, PropertyChangedEventArgs ev)
    {
        if (ev.PropertyName == nameof(Router.CurrentPageViewModel))
        {
            NavigateToViewModel(Router!.CurrentPageViewModel);
        }
    }

    public RoutingState? Router
    {
        get => GetValue(RouterProperty);
        set => SetValue(RouterProperty, value);
    }

    public object? DefaultContent
    {
        get => GetValue(DefaultContentProperty);
        set => SetValue(DefaultContentProperty, value);
    }

    public IViewLocator? ViewLocator { get; set; }

    protected override Type StyleKeyOverride => typeof(TransitioningContentControl);

    private void NavigateToViewModel(ViewModelBase? viewModelBase)
    {
        if (Router == null)
        {
            Content = DefaultContent;
            return;
        }

        if (viewModelBase == null)
        {
            Content = DefaultContent;
            return;
        }

        var viewLocator = ViewLocator ??= ViewLocators.ViewLocator.DefaultViewLocator;
        var viewInstance = viewLocator.Build(viewModelBase);
        if (viewInstance == null)
        {
            Content = DefaultContent;
            return;
        }

        viewInstance.DataContext = viewModelBase;
        if (viewInstance is IDataContextProvider provider)
        {
            provider.DataContext = viewModelBase;
        }

        Content = viewInstance;
    }
}