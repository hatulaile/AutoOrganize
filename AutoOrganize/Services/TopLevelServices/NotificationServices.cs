using System;
using System.Collections.Concurrent;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;

namespace AutoOrganize.Services.TopLevelServices;

public sealed class NotificationServices : TopLevelServicesBase<INotificationManager>, INotificationServices
{
    private readonly ConcurrentDictionary<TopLevel, WindowsNotificationState>
        _notificationManagerCache = new();

    public void Show(INotification notification, Visual visual) =>
        ExecuteWhenManagerReady(visual, manager => manager.Show(notification));

    public void Show(INotification notification, object? dataContext = null) =>
        ExecuteWhenManagerReady(dataContext, manager => manager.Show(notification));

    public void Close(INotification notification, Visual visual) =>
        ExecuteWhenManagerReady(visual, manager => manager.Close(notification));

    public void Close(INotification notification, object? dataContext = null) =>
        ExecuteWhenManagerReady(dataContext, manager => manager.Close(notification));

    public void CLoseAll(Visual visual) =>
        ExecuteWhenManagerReady(visual, manager => manager.CloseAll());

    public void CLoseAll(object? dataContext = null) =>
        ExecuteWhenManagerReady(dataContext, manager => manager.CloseAll());

    private void ExecuteWhenManagerReady(Visual visual, Action<INotificationManager> action) =>
        ExecuteWhenManagerReady(GetTopLevel(visual), action);

    private void ExecuteWhenManagerReady(object? dataContext, Action<INotificationManager> action) =>
        ExecuteWhenManagerReady(
            dataContext is not null ? FindTopLevel(dataContext) ?? DefaultTopLevel : DefaultTopLevel, action);

    private void ExecuteWhenManagerReady(TopLevel topLevel, Action<INotificationManager> action)
    {
        (var manager, bool isNew) = _notificationManagerCache.GetOrAdd(topLevel, top =>
        {
            top.Closed += (_, _) => _notificationManagerCache.TryRemove(top, out _);
            var manager = new WindowNotificationManager(top) { MaxItems = 5 };
            manager.TemplateApplied += (_, _) =>
                _notificationManagerCache[top] = _notificationManagerCache[top] with { IsNew = false };
            return new WindowsNotificationState(manager, true);
        });

        if (isNew)
        {
            manager.TemplateApplied += (_, _) => action(manager);
            return;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            action(manager);
            return;
        }

        Dispatcher.UIThread.Post(() => action(manager));
    }

    protected override INotificationManager GetProvider(TopLevel topLevel)
    {
        return _notificationManagerCache.GetOrAdd(topLevel, top =>
        {
            top.Closed += (_, _) => _notificationManagerCache.TryRemove(top, out _);
            var manager = new WindowNotificationManager(top) { MaxItems = 5 };
            manager.TemplateApplied += (_, _) =>
                _notificationManagerCache[top] = _notificationManagerCache[top] with { IsNew = false };
            return new WindowsNotificationState(manager, true);
        }).Manager;
    }

    private sealed record WindowsNotificationState(WindowNotificationManager Manager, bool IsNew);
}