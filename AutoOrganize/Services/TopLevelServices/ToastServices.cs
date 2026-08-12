using System;
using System.Collections.Concurrent;
using AutoOrganize.Services.WindowManagers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Ursa.Controls;

namespace AutoOrganize.Services.TopLevelServices;

public sealed class ToastServices : TopLevelServicesBase<IToastManager>, IToastServices
{
    private readonly ConcurrentDictionary<TopLevel, WindowsToastState> _toastManagerCache = new();

    public void Show(IToast toast, Visual visual) =>
        ExecuteWhenManagerReady(visual, manager => manager.Show(toast));

    public void Show(IToast toast, object? dataContext = null) =>
        ExecuteWhenManagerReady(dataContext, manager => manager.Show(toast));

    public void Close(IToast toast, Visual visual) =>
        ExecuteWhenManagerReady(visual, manager => manager.Close(toast));

    public void Close(IToast toast, object? dataContext = null) =>
        ExecuteWhenManagerReady(dataContext, manager => manager.Close(toast));

    public void CloseAll(Visual visual) =>
        ExecuteWhenManagerReady(visual, manager => manager.CloseAll());

    public void CloseAll(object? dataContext = null) =>
        ExecuteWhenManagerReady(dataContext, manager => manager.CloseAll());

    private void ExecuteWhenManagerReady(Visual visual, Action<IToastManager> action) =>
        ExecuteWhenManagerReady(GetTopLevel(visual), action);

    private void ExecuteWhenManagerReady(object? dataContext, Action<IToastManager> action) =>
        ExecuteWhenManagerReady(
            dataContext is not null ? FindTopLevel(dataContext) ?? DefaultTopLevel : DefaultTopLevel, action);

    private void ExecuteWhenManagerReady(TopLevel topLevel, Action<IToastManager> action)
    {
        (var manager, bool isNew) = _toastManagerCache.GetOrAdd(topLevel, top =>
        {
            EventHandler? windowOnClosed = null;
            windowOnClosed = (sender, ev) =>
            {
                _toastManagerCache.TryRemove(top, out _);
                if (sender is Window window)
                    window.Closed -= windowOnClosed;
            };
            top.Closed += windowOnClosed;

            var manager = new WindowToastManager(top) { MaxItems = 5 };
            EventHandler<TemplateAppliedEventArgs>? windowTemplateApplied = null;
            windowTemplateApplied = (sender, _) =>
            {
                _toastManagerCache[top] = _toastManagerCache[top] with { IsNew = false };
                if (sender is Window window)
                    window.TemplateApplied -= windowTemplateApplied;
            };
            manager.TemplateApplied += windowTemplateApplied;
            return new WindowsToastState(manager, true);
        });

        if (isNew)
        {
            EventHandler<TemplateAppliedEventArgs>? windowTemplateApplied = null;
            windowTemplateApplied = (sender, _) =>
            {
                action(manager);
                if (sender is Window window)
                    window.TemplateApplied -= windowTemplateApplied;
            };
            manager.TemplateApplied += windowTemplateApplied;
            return;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            action(manager);
            return;
        }

        Dispatcher.UIThread.Post(() => action(manager));
    }

    protected override IToastManager GetProvider(TopLevel topLevel)
    {
        return _toastManagerCache.GetOrAdd(topLevel, top =>
        {
            EventHandler? windowOnClosed = null;
            windowOnClosed = (sender, ev) =>
            {
                _toastManagerCache.TryRemove(top, out _);
                if (sender is Window window)
                    window.Closed -= windowOnClosed;
            };
            top.Closed += windowOnClosed;

            var manager = new WindowToastManager(top) { MaxItems = 5 };
            EventHandler<TemplateAppliedEventArgs>? windowTemplateApplied = null;
            windowTemplateApplied = (sender, _) =>
            {
                _toastManagerCache[top] = _toastManagerCache[top] with { IsNew = false };
                if (sender is Window window)
                    window.TemplateApplied -= windowTemplateApplied;
            };
            manager.TemplateApplied += windowTemplateApplied;

            return new WindowsToastState(manager, true);
        }).Manager;
    }

    private sealed record WindowsToastState(WindowToastManager Manager, bool IsNew);

    public ToastServices(IWindowProvider windowProvider) : base(windowProvider)
    {
    }
}