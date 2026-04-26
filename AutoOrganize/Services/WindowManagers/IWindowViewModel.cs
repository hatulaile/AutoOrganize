namespace AutoOrganize.Services.WindowManagers;

public interface IWindowViewModel
{
    void OnOpenWindow()
    {
    }

    void OnCloseWindow()
    {
    }
}

public interface IResultWindowViewModel<out TResult> : IWindowViewModel;

public interface IWindowViewModel<in TArgs> : IWindowViewModel
{
    void OnOpenWindow(TArgs args)
    {
    }
}

public interface IWindowViewModel<in TArgs, out TResult> : IWindowViewModel<TArgs>, IResultWindowViewModel<TResult>
{
}