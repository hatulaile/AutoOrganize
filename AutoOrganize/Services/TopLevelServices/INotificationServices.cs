using Avalonia;
using Avalonia.Controls.Notifications;

namespace AutoOrganize.Services.TopLevelServices;

public interface INotificationServices
{
    void Show(INotification notification, Visual visual);

    void Show(INotification notification, object? dataContext = null);

    void Close(INotification notification, Visual visual);

    void Close(INotification notification, object? dataContext = null);

    void CLoseAll(Visual visual);

    void CLoseAll(object? dataContext = null);
}