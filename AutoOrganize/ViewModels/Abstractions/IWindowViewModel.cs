namespace AutoOrganize.ViewModels.Abstractions;

public interface IWindowViewModel : IViewModel
{
    bool AllowMultipleInstances => false;

    void OnOpenWindow()
    {
    }

    void OnCloseWindow()
    {
    }
}

public interface IWindowViewModel<in TArgs> : IWindowViewModel
{
    void OnOpenWindow(TArgs args)
    {
    }
}