using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace AutoOrganize.Services.WindowManagers;

public partial class WindowService : IWindowService, IWindowProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<object, Window> _windowByViewModel = [];

    public Window MainWindow { get; }

    public IReadOnlyList<Window> Windows => (IReadOnlyList<Window>)_windowByViewModel.Values;

    public WindowService(IServiceProvider serviceProvider)
    {
        if (App.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktopLifetime)
            throw new Exception("当前应用程序不支持经典桌面样式, 无法初始化窗口服务!");
        _serviceProvider = serviceProvider;

        MainWindow = desktopLifetime.MainWindow ?? throw new Exception("当前应用程序的主窗口未设置, 无法初始化窗口服务!");
    }
}