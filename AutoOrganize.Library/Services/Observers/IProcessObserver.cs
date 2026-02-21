namespace AutoOrganize.Library.Services.Observers;

public interface IProcessObserver : IProcessObserver<object>
{
    void OnSuccess();

    void IProcessObserver<object, object, Exception>.OnSuccess(object info)
    {
        OnSuccess();
    }
}

public interface IProcessObserver<in TInfo> : IProcessObserver<TInfo, object>
{
    void OnCompleted();

    void IProcessObserver<TInfo, object, Exception>.OnCompleted(object result)
    {
        OnCompleted();
    }
}

public interface IProcessObserver<in TInfo, in TResult> : IProcessObserver<TInfo, TResult, Exception>;

public interface IProcessObserver<in TInfo, in TResult, in TError>
{
    void OnSuccess(TInfo info);

    void OnFailure(TError ex);

    void OnCompleted(TResult result);
}

public class ProcessObserver : IProcessObserver
{
    public event Action? Success;
    public event Action<Exception>? Failure;
    public event Action? Completed;

    public void OnSuccess()
    {
        Success?.Invoke();
    }

    public void OnFailure(Exception ex)
    {
        Failure?.Invoke(ex);
    }

    public void OnCompleted()
    {
        Completed?.Invoke();
    }
}

public class ProcessObserver<TInfo> : IProcessObserver<TInfo>
{
    public event Action<TInfo>? Success;
    public event Action<Exception>? Failure;
    public event Action? Completed;

    public void OnSuccess(TInfo info)
    {
        Success?.Invoke(info);
    }

    public void OnFailure(Exception ex)
    {
        Failure?.Invoke(ex);
    }

    public void OnCompleted()
    {
        Completed?.Invoke();
    }
}

public class ProcessObserver<TInfo, TResult> : IProcessObserver<TInfo, TResult>
{
    public event Action<TInfo>? Success;
    public event Action<Exception>? Failure;
    public event Action<TResult>? Completed;

    public void OnSuccess(TInfo info)
    {
        Success?.Invoke(info);
    }

    public void OnFailure(Exception ex)
    {
        Failure?.Invoke(ex);
    }

    public void OnCompleted(TResult result)
    {
        Completed?.Invoke(result);
    }
}

public class ProcessObserver<TInfo, TResult, TError> : IProcessObserver<TInfo, TResult, TError>
{
    public event Action<TInfo>? Success;
    public event Action<TError>? Failure;
    public event Action<TResult>? Completed;

    public void OnSuccess(TInfo info)
    {
        Success?.Invoke(info);
    }

    public void OnFailure(TError ex)
    {
        Failure?.Invoke(ex);
    }

    public void OnCompleted(TResult result)
    {
        Completed?.Invoke(result);
    }
}