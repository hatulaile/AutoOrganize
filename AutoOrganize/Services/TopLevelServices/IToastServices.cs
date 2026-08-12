using Avalonia;
using Ursa.Controls;

namespace AutoOrganize.Services.TopLevelServices;

public interface IToastServices
{
    void Show(IToast toast, Visual visual);

    void Show(IToast toast, object? dataContext = null);

    void Close(IToast toast, Visual visual);

    void Close(IToast toast, object? dataContext = null);

    void CloseAll(Visual visual);

    void CloseAll(object? dataContext = null);
}
