using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.Services.WindowManagers;

public partial class WindowService : IWindowService, IWindowProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<object, Window> _windowByViewModel = [];
    private bool _isMainClosing;
    private bool _isMainClosed;

    public IReadOnlyList<Window> Windows => (IReadOnlyList<Window>)_windowByViewModel.Values;

    private Window MainWindow
    {
        get
        {
            if (field is not null)
                return field;

            field = ((IClassicDesktopStyleApplicationLifetime?)App.Current.ApplicationLifetime)?.MainWindow ??
                    throw new Exception("主窗口未找到, 无法初始化窗口服务!");
            InitMainWindow(field);
            return field;
        }
    }

    public WindowService(IServiceProvider serviceProvider)
    {
        if (App.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime)
            throw new Exception("当前应用程序不支持经典桌面样式, 无法初始化窗口服务!");

        _serviceProvider = serviceProvider;

        if (lifetime.MainWindow is not null)
        {
            MainWindow = lifetime.MainWindow;
            InitMainWindow(MainWindow);
        }
    }

    private static ViewModelLifetime GetWindowLifetime(Window window)
    {
        if (window.DataContext is null)
            return ViewModelLifetime.None;

        var attribute = window.DataContext.GetType().GetCustomAttribute<ViewModelRegistrationAttribute>();
        return attribute?.ViewLifetime ?? ViewModelLifetime.None;
    }

    private void InitMainWindow(Window window)
    {
        window.ClosingBehavior = WindowClosingBehavior.OwnerWindowOnly;
        window.Closing += OnMainWindowClosing;
        window.Closed += WindowOnClosed;

        EventHandler<RoutedEventArgs>? handler;
        handler = (sender, ev) => _windowByViewModel.TryAdd(window.DataContext!, window);
        window.Loaded += handler;
    }

    private static bool CanCloseWindow(Window window)
    {
        return GetWindowLifetime(window) is ViewModelLifetime.Transient;
    }

    private void OnMainWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        _isMainClosing = true;
        foreach (Window window in _windowByViewModel.Values.ToArray())
        {
            if (ReferenceEquals(window, MainWindow))
                continue;
            window.Close();
        }

        if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            foreach (Window window in lifetime.Windows.ToArray())
            {
                if (ReferenceEquals(window, MainWindow))
                    continue;
                window.Close();
            }
        }
    }

    private void WindowOnClosed(object? sender, EventArgs e)
    {
        _isMainClosed = true;
    }
}