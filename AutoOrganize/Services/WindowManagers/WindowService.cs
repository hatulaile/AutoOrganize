using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.Services.WindowManagers;

public partial class WindowService : IWindowService, IWindowProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<object, Window> _windowByViewModel = [];

    public static Window MainWindow =>
        ((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime!).MainWindow!;

    public IReadOnlyList<Window> Windows => (IReadOnlyList<Window>)_windowByViewModel.Values;

    public WindowService(IServiceProvider serviceProvider)
    {
        if (App.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime)
            throw new Exception("当前应用程序不支持经典桌面样式, 无法初始化窗口服务!");
        _serviceProvider = serviceProvider;
    }

    private static ViewModelLifetime GetWindowLifetime(Window window)
    {
        if (window.DataContext is null)
            return ViewModelLifetime.None;

        var attribute = window.DataContext.GetType().GetCustomAttribute<ViewModelRegistrationAttribute>();
        return attribute?.ViewLifetime ?? ViewModelLifetime.None;
    }

    private static bool CanCloseWindow(Window window)
    {
        return GetWindowLifetime(window) is ViewModelLifetime.Transient;
    }
}